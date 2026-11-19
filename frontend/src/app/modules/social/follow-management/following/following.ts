import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowCountsDto, FollowingDto } from '../../api/social-api-types';
import { FollowManagement } from '../follow-management';
import { FollowingCard } from '../following-card/following-card';

const PAGE_SIZE = 20;

@Component({
  imports: [FollowingCard, RouterLink],
  selector: 'app-following',
  styleUrl: './following.scss',
  templateUrl: './following.html',
})
export class Following {
  private readonly followManagement = inject(FollowManagement);

  // Changing the page changes the address, so the resource loads that page.
  protected readonly page = signal(1);

  protected readonly following = httpResource<PageResult<FollowingDto>>(
    () => `/api/social/following?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly counts = httpResource<FollowCountsDto>(() => '/api/social/follow-counts');

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.following.value()?.totalCount ?? 0) / PAGE_SIZE)),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async stop(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followManagement.stopFollowing(requestId);
      this.following.reload();
      this.counts.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'You are still following this user.'));
    } finally {
      this.pending.set(false);
    }
  }
}
