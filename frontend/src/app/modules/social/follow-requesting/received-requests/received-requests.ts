import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ReceivedFollowRequestDto } from '../../api/social-api-types';
import { FollowRequesting } from '../follow-requesting';
import { ReceivedRequestCard } from '../received-request-card/received-request-card';

const PAGE_SIZE = 20;

@Component({
  imports: [ReceivedRequestCard, RouterLink],
  selector: 'app-received-requests',
  styleUrl: './received-requests.scss',
  templateUrl: './received-requests.html',
})
export class ReceivedRequests {
  private readonly followRequesting = inject(FollowRequesting);

  // Changing the page changes the address, so the resource loads that page.
  protected readonly page = signal(1);

  protected readonly requests = httpResource<PageResult<ReceivedFollowRequestDto>>(
    () => `/api/social/follow-requests/received?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.requests.value()?.totalCount ?? 0) / PAGE_SIZE)),
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
