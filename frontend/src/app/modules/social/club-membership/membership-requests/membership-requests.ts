import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto, MembershipRequestDto } from '../../api/social-api-types';
import { ClubMembership } from '../club-membership';

@Component({
  imports: [RouterLink],
  selector: 'app-membership-requests',
  styleUrl: './membership-requests.scss',
  templateUrl: './membership-requests.html',
})
export class MembershipRequests {
  private readonly clubMembership = inject(ClubMembership);

  readonly id = input.required<string>();

  // The club is read only for its name in the back link and the header.
  protected readonly club = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);

  protected readonly requests = httpResource<MembershipRequestDto[]>(
    () => `/api/social/clubs/${this.id()}/membership-requests`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(requesterId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubMembership.accept(this.id(), requesterId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the request.'));
    } finally {
      this.pending.set(false);
      this.requests.reload();
    }
  }

  protected async reject(requesterId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubMembership.reject(this.id(), requesterId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reject the request.'));
    } finally {
      this.pending.set(false);
      this.requests.reload();
    }
  }

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }
}
