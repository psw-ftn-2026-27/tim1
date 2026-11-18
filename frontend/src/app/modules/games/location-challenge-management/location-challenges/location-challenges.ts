import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeSummaryDto } from '../../api/games-api-types';
import { LocationChallengeManagement } from '../location-challenge-management';

const PAGE_SIZE = 10;

@Component({
  imports: [RouterLink],
  selector: 'app-location-challenges',
  styleUrl: './location-challenges.scss',
  templateUrl: './location-challenges.html',
})
export class LocationChallenges {
  private readonly locationChallengeManagement = inject(LocationChallengeManagement);

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<LocationChallengeSummaryDto>>(
    () => `/api/games/location-challenges?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly items = computed(() => this.challenges.value()?.items ?? []);
  protected readonly totalCount = computed(() => this.challenges.value()?.totalCount ?? 0);
  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)),
  );

  // The numbers on the page buttons in the table footer: 1, 2, ... totalPages.
  protected readonly pageNumbers = computed(() => {
    const numbers: number[] = [];
    for (let number = 1; number <= this.totalPages(); number++) {
      numbers.push(number);
    }
    return numbers;
  });

  // For "11–20 of 34 challenges": the positions of the first and the last row on this page.
  protected readonly firstShown = computed(() => (this.page() - 1) * PAGE_SIZE + 1);
  protected readonly lastShown = computed(
    () => (this.page() - 1) * PAGE_SIZE + this.items().length,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly deletingId = signal<string | null>(null);

  protected goToPage(page: number): void {
    this.deletingId.set(null);
    this.page.set(page);
  }

  protected async publish(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.publish(id);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the challenge.'));
    } finally {
      this.challenges.reload();
      this.pending.set(false);
    }
  }

  protected async retire(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.retire(id);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not retire the challenge.'));
    } finally {
      this.challenges.reload();
      this.pending.set(false);
    }
  }

  protected async archive(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.archive(id);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not archive the challenge.'));
    } finally {
      this.challenges.reload();
      this.pending.set(false);
    }
  }

  protected startDelete(id: string): void {
    this.error.set(null);
    this.deletingId.set(id);
  }

  protected cancelDelete(): void {
    this.deletingId.set(null);
  }

  protected async confirmDelete(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.delete(id);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the challenge.'));
    } finally {
      this.deletingId.set(null);
      this.challenges.reload();
      this.pending.set(false);
    }
  }
}
