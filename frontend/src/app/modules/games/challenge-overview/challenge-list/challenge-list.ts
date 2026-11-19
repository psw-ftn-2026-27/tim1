import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { formatDate } from '../../../../shared/util/format-date';
import { ChallengeOverviewDto } from '../../api/games-api-types';

const PAGE_SIZE = 10;
const emptyFilters = { name: '', type: '', status: '' };

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly auth = inject(Auth);
  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly formatDate = formatDate;

  // For each challenge type the server sends, the route section that holds its edit page.
  protected readonly typePaths: Record<string, string> = {
    Quiz: 'quizzes',
    SocialChallenge: 'social-challenges',
    LocationChallenge: 'location-challenges',
  };
  protected readonly page = signal(1);

  // The list refreshes while the user types the name, but only after a short pause (300 ms),
  // so the server does not get a request for every key.
  protected readonly filters = form(signal({ ...emptyFilters }), (path) => {
    debounce(path.name, 300);
  });

  protected readonly challenges = httpResource<ChallengeOverviewDto>(() => {
    if (!this.isAdministrator()) {
      return undefined;
    }

    const { name, type, status } = this.filters().value();
    const params = new URLSearchParams({
      page: String(this.page()),
      pageSize: String(PAGE_SIZE),
    });
    if (name.trim() !== '') {
      params.set('name', name.trim());
    }
    if (type !== '') {
      params.set('type', type);
    }
    if (status !== '') {
      params.set('status', status);
    }
    return `/api/games/challenges?${params.toString()}`;
  });

  // For "11–20 of 34 challenges": the positions of the first and the last row on this page.
  protected readonly firstShown = computed(() => (this.page() - 1) * PAGE_SIZE + 1);

  protected lastShown(rowsOnPage: number): number {
    return (this.page() - 1) * PAGE_SIZE + rowsOnPage;
  }

  protected totalPages(totalCount: number): number {
    return Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  }

  // The numbers on the page buttons in the table footer: 1, 2, ... totalPages.
  protected pageNumbers(totalCount: number): number[] {
    const numbers: number[] = [];
    for (let number = 1; number <= this.totalPages(totalCount); number++) {
      numbers.push(number);
    }
    return numbers;
  }

  protected clearFilters(): void {
    this.filters().reset({ ...emptyFilters });
    this.page.set(1);
  }
}
