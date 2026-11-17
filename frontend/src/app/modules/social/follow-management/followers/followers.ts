import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowerDto } from '../../api/social-api-types';
import { FollowManagement } from '../follow-management';
import { FollowerCard } from '../follower-card/follower-card';

@Component({
  imports: [FollowerCard, RouterLink],
  selector: 'app-followers',
  styleUrl: './followers.scss',
  templateUrl: './followers.html',
})
export class Followers {
  private readonly followManagement = inject(FollowManagement);

  protected readonly followers = httpResource<PageResult<FollowerDto>>(
    () => '/api/social/followers?page=1&pageSize=20',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async remove(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followManagement.removeFollower(requestId);
      this.followers.reload();
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
