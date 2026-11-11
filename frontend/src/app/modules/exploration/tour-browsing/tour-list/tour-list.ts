import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { TourDto } from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';
import { TourPagination } from '../tour-pagination/tour-pagination';
import { TourCriteria, TourSearch } from '../tour-search/tour-search';

const PAGE_SIZE = 20;

@Component({
  imports: [RouterLink, TourCard, TourPagination, TourSearch],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  protected readonly pageSize = PAGE_SIZE;

  // The criteria that are actually sent to the server.
  private readonly criteria = signal<TourCriteria>({
    title: '',
    difficulties: [],
    transports: [],
    tags: [],
    sort: 'Newest',
  });
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

  protected readonly tours = httpResource<PageResult<TourDto>>(
    () => `/api/exploration/tours/published?${this.query()}`,
  );

  protected applyCriteria(criteria: TourCriteria): void {
    this.criteria.set(criteria);
    this.page.set(1);
  }

  protected goToPage(page: number): void {
    this.page.set(page);
  }
}
