import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { FollowRequestDto } from '../../api/social-api-types';
import { FollowRequestCard } from '../follow-request-card/follow-request-card';
import { FollowRequesting } from '../follow-requesting';

@Component({
  imports: [FollowRequestCard, RouterLink],
  selector: 'app-follow-requests',
  styleUrl: './follow-requests.scss',
  templateUrl: './follow-requests.html',
})
export class FollowRequests {
  private readonly followRequesting = inject(FollowRequesting);

  protected readonly requests = httpResource<FollowRequestDto[]>(
    () => '/api/social/follow-requests/sent',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async delete(requestId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequesting.delete(requestId);
      this.requests.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije obrisan.'));
    } finally {
      this.pending.set(false);
    }
  }
}
