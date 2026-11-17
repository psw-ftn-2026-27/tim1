import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto, ClubMemberDto, MyMembershipDto } from '../../api/social-api-types';
import { ClubBrowsing } from '../club-browsing';

@Component({
  imports: [RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubBrowsing = inject(ClubBrowsing);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);
  protected readonly membership = httpResource<MyMembershipDto>(
    () => `/api/social/clubs/${this.id()}/membership`,
  );
  protected readonly members = httpResource<ClubMemberDto[]>(
    () => `/api/social/clubs/${this.id()}/members`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async join(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubBrowsing.requestMembership(this.id());
      this.membership.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the membership request.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async withdraw(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubBrowsing.withdrawMembershipRequest(this.id());
      this.membership.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not withdraw the membership request.'));
    } finally {
      this.pending.set(false);
    }
  }
}
