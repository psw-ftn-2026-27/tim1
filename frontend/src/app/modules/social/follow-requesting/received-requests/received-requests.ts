import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedFollowRequestDto } from '../../api/social-api-types';
import { FollowRequesting } from '../follow-requesting';
import { ReceivedRequestCard } from '../received-request-card/received-request-card';

@Component({
  imports: [ReceivedRequestCard, RouterLink],
  selector: 'app-received-requests',
  styleUrl: './received-requests.scss',
  templateUrl: './received-requests.html',
})
export class ReceivedRequests {
  private readonly followRequesting = inject(FollowRequesting);

  protected readonly requests = httpResource<PageResult<ReceivedFollowRequestDto>>(
    () => '/api/social/follow-requests/received?page=1&pageSize=20',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequesting.accept(requestId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not accepted.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reject(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequesting.reject(requestId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not rejected.'));
    } finally {
      this.pending.set(false);
    }
  }
}
