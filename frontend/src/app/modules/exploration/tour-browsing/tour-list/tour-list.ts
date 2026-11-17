import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { TourDto, TourPreferenceDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';
import { TourPagination } from '../tour-pagination/tour-pagination';
import { TourCriteria, TourSearch } from '../tour-search/tour-search';

const PAGE_SIZE = 20;

const EMPTY_CRITERIA: TourCriteria = {
  title: '',
  difficulties: [],
  transports: [],
  tags: [],
  sort: 'Newest',
};

@Component({
  imports: [RouterLink, TourCard, TourPagination, TourSearch],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  protected readonly pageSize = PAGE_SIZE;

  // Only a logged-in user has preferences; for a guest the address is undefined and nothing is requested.
  private readonly preferences = httpResource<TourPreferenceDto | null>(() =>
    this.auth.isLoggedIn() ? '/api/exploration/tour-preferences/mine' : undefined,
  );

  // True once the preferences request is finished (or was never made), so the tours can be requested.
  protected readonly preferencesSettled = computed(() => !this.preferences.isLoading());

  // The preferences as criteria, only when the user marked them to be used for search.
  private readonly preferenceCriteria = computed<TourCriteria | null>(() => {
    const preferences = this.preferences.hasValue() ? this.preferences.value() : null;
    if (preferences?.opensSearch !== true) {
      return null;
    }
    return {
      title: '',
      difficulties: preferences.difficulties,
      transports: preferences.transport === null ? [] : [preferences.transport],
      tags: preferences.tags,
      sort: 'Newest',
    };
  });

  // The criteria from the search form. It is null until the user changes the search, and then the preferences apply.
  private readonly userCriteria = signal<TourCriteria | null>(null);

  // The criteria that are actually sent to the server. The search form shows exactly these.
  protected readonly criteria = computed(
    () => this.userCriteria() ?? this.preferenceCriteria() ?? EMPTY_CRITERIA,
  );
  protected readonly page = signal(1);

  private readonly query = computed(() => {
    const c = this.criteria();
    const params = new URLSearchParams();

    if (c.title !== '') {
      params.append('title', c.title);
    }
    for (const difficulty of c.difficulties) {
      params.append('difficulties', difficulty);
    }
    for (const transport of c.transports) {
      params.append('transports', transport);
    }
    for (const tag of c.tags) {
      params.append('tags', tag);
    }
    params.append('sort', c.sort);
    params.append('page', String(this.page()));
    params.append('pageSize', String(PAGE_SIZE));

    return params.toString();
  });

  // Not requested until the preferences are settled, so the first request already has the default filter.
  protected readonly tours = httpResource<PageResult<TourDto>>(() =>
    this.preferencesSettled() ? `/api/exploration/tours/published?${this.query()}` : undefined,
  );

  protected applyCriteria(criteria: TourCriteria): void {
    this.userCriteria.set(criteria);
    this.page.set(1);
  }

  protected goToPage(page: number): void {
    this.page.set(page);
  }
}
