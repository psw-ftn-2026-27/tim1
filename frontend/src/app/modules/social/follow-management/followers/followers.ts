import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowCountsDto, FollowerDto } from '../../api/social-api-types';
import { FollowManagement } from '../follow-management';
import { FollowerCard } from '../follower-card/follower-card';

const PAGE_SIZE = 20;

@Component({
  imports: [FollowerCard, RouterLink],
  selector: 'app-followers',
  styleUrl: './followers.scss',
  templateUrl: './followers.html',
})
export class Followers {
  private readonly followManagement = inject(FollowManagement);

  // Changing the page changes the address, so the resource loads that page.
  protected readonly page = signal(1);

  protected readonly followers = httpResource<PageResult<FollowerDto>>(
    () => `/api/social/followers?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly counts = httpResource<FollowCountsDto>(() => '/api/social/follow-counts');

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.followers.value()?.totalCount ?? 0) / PAGE_SIZE)),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async remove(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followManagement.removeFollower(requestId);
      this.followers.reload();
      this.counts.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Follower was not removed.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async send(recipientId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followManagement.sendFollowRequest(recipientId);
      this.followers.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not sent.'));
    } finally {
      this.pending.set(false);
    }
  }
}
