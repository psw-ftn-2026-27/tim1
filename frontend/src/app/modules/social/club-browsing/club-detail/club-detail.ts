import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
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

  protected readonly isOwner = computed(() => this.membership.value()?.role === 'Owner');
  protected readonly canSeeRequests = computed(() => {
    const role = this.membership.value()?.role;
    return role === 'Owner' || role === 'Moderator';
  });
  protected readonly roleError = signal<string | null>(null);
  protected readonly transferringToUserId = signal<string | null>(null);

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

  protected async promote(memberId: string): Promise<void> {
    this.pending.set(true);
    this.roleError.set(null);
    this.transferringToUserId.set(null);
    try {
      await this.clubBrowsing.promoteToModerator(this.id(), memberId);
      this.membership.reload();
      this.members.reload();
    } catch (failure) {
      this.roleError.set(serverMessage(failure, 'Could not make the member a moderator.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startTransfer(memberId: string): void {
    this.roleError.set(null);
    this.transferringToUserId.set(memberId);
  }

  protected cancelTransfer(): void {
    this.transferringToUserId.set(null);
  }

  protected async confirmTransfer(memberId: string): Promise<void> {
    this.pending.set(true);
    this.roleError.set(null);
    try {
      await this.clubBrowsing.transferOwnership(this.id(), memberId);
      this.transferringToUserId.set(null);
      this.membership.reload();
      this.members.reload();
    } catch (failure) {
      this.transferringToUserId.set(null);
      this.roleError.set(serverMessage(failure, 'Could not transfer the ownership.'));
    } finally {
      this.pending.set(false);
    }
  }
}
