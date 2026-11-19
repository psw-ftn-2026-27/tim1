import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { ChallengeOverviewDto } from '../../api/games-api-types';

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
  // For each challenge type the server sends: its name on the page, and the route section
  // that holds its edit page.
  protected readonly typeLabels: Record<string, string> = {
    Quiz: 'Quiz',
    SocialChallenge: 'Social challenge',
    LocationChallenge: 'Location challenge',
  };
  protected readonly typePaths: Record<string, string> = {
    Quiz: 'quizzes',
    SocialChallenge: 'social-challenges',
    LocationChallenge: 'location-challenges',
  };
  protected readonly pageSize = 20;
  protected readonly page = signal(1);
  protected readonly filters = form(signal({ ...emptyFilters }));
  protected readonly hasFilters = computed(() => {
    const { name, type, status } = this.filters().value();
    return name !== '' || type !== '' || status !== '';
  });

  protected readonly challenges = httpResource<ChallengeOverviewDto>(() => {
    if (!this.isAdministrator()) {
      return undefined;
    }

    const { name, type, status } = this.filters().value();
    const params = new URLSearchParams({
      page: String(this.page()),
      pageSize: String(this.pageSize),
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

  protected clearFilters(): void {
    this.filters().reset({ ...emptyFilters });
    this.page.set(1);
  }
}
