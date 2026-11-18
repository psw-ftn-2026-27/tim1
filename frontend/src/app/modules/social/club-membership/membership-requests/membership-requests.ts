import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { MembershipRequestDto } from '../../api/social-api-types';
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
}
