import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeManagement } from '../social-challenge-management';

const PAGE_SIZE = 10;

@Component({
  imports: [RouterLink],
  selector: 'app-social-challenges',
  styleUrl: './social-challenges.scss',
  templateUrl: './social-challenges.html',
})
export class SocialChallenges {
  private readonly socialChallengeManagement = inject(SocialChallengeManagement);

  protected readonly page = signal(1);

  protected readonly challenges = httpResource<PageResult<SocialChallengeDto>>(
    () => `/api/games/social-challenges?page=${this.page()}&pageSize=${PAGE_SIZE}`,
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
  protected readonly deletingId = signal<string | null>(null);
  // The last refused action: which challenge, what was tried, and the server's message.
  protected readonly failure = signal<{ name: string; verb: string; message: string } | null>(
    null,
  );

  // The server sends "HH:mm:ss"; the page shows only hours and minutes (24-hour time).
  protected hoursAndMinutes(time: string): string {
    return time.slice(0, 5);
  }

  protected goToPage(page: number): void {
    this.deletingId.set(null);
    this.page.set(page);
  }

  protected publish(challenge: SocialChallengeDto): Promise<void> {
    return this.run(challenge, 'published', () =>
      this.socialChallengeManagement.publish(challenge.id),
    );
  }

  protected retire(challenge: SocialChallengeDto): Promise<void> {
    return this.run(challenge, 'retired', () =>
      this.socialChallengeManagement.retire(challenge.id),
    );
  }

  protected archive(challenge: SocialChallengeDto): Promise<void> {
    return this.run(challenge, 'archived', () =>
      this.socialChallengeManagement.archive(challenge.id),
    );
  }

  protected startDelete(id: string): void {
    this.failure.set(null);
    this.deletingId.set(id);
  }

  protected cancelDelete(): void {
    this.deletingId.set(null);
  }

  protected confirmDelete(challenge: SocialChallengeDto): Promise<void> {
    return this.run(challenge, 'deleted', () =>
      this.socialChallengeManagement.delete(challenge.id),
    );
  }

  // Runs one command; a refusal shows the server's message above the table. Either way the
  // list is loaded again, so the actions match the challenge's current state.
  private async run(
    challenge: SocialChallengeDto,
    verb: string,
    command: () => Promise<void>,
  ): Promise<void> {
    this.pending.set(true);
    this.failure.set(null);
    try {
      await command();
    } catch (error) {
      this.failure.set({
        name: challenge.name,
        verb,
        message: serverMessage(error, 'the server refused the change.'),
      });
    } finally {
      this.deletingId.set(null);
      this.challenges.reload();
      this.pending.set(false);
    }
  }
}
