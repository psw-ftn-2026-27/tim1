import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowingDto } from '../../api/social-api-types';
import { FollowManagement } from '../follow-management';
import { FollowingCard } from '../following-card/following-card';

@Component({
  imports: [FollowingCard, RouterLink],
  selector: 'app-following',
  styleUrl: './following.scss',
  templateUrl: './following.html',
})
export class Following {
  private readonly followManagement = inject(FollowManagement);

  protected readonly following = httpResource<PageResult<FollowingDto>>(
    () => '/api/social/following?page=1&pageSize=20',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async stop(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followManagement.stopFollowing(requestId);
      this.following.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'You are still following this user.'));
    } finally {
      this.pending.set(false);
    }
  }
}
