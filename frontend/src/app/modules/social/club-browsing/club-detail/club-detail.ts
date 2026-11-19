import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  ClubDto,
  ClubMemberDto,
  ClubRole,
  MembershipRequestDto,
  MyMembershipDto,
} from '../../api/social-api-types';
import { ClubBrowsing } from '../club-browsing';

@Component({
  imports: [RouterLink],
  selector: 'app-club-detail',
  styleUrl: './club-detail.scss',
  templateUrl: './club-detail.html',
})
export class ClubDetail {
  private readonly clubBrowsing = inject(ClubBrowsing);
  protected readonly auth = inject(Auth);

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
  protected readonly isModerator = computed(() => this.membership.value()?.role === 'Moderator');

  // Only the owner and moderators may read the pending requests; the others never ask.
  protected readonly requests = httpResource<MembershipRequestDto[]>(() =>
    this.canSeeRequests() ? `/api/social/clubs/${this.id()}/membership-requests` : undefined,
  );

  // Index of the image shown large in the gallery.
  protected readonly activeImage = signal(0);
  protected readonly roleError = signal<string | null>(null);
  protected readonly transferringToUserId = signal<string | null>(null);
  protected readonly removingUserId = signal<string | null>(null);

  protected canRemove(memberRole: ClubRole): boolean {
    if (this.isOwner()) {
      return memberRole !== 'Owner';
    }
    if (this.isModerator()) {
      return memberRole === 'Member';
    }
    return false;
  }

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
    this.removingUserId.set(null);
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
    this.removingUserId.set(null);
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

  protected startRemove(memberId: string): void {
    this.roleError.set(null);
    this.transferringToUserId.set(null);
    this.removingUserId.set(memberId);
  }

  protected cancelRemove(): void {
    this.removingUserId.set(null);
  }

  protected async confirmRemove(memberId: string): Promise<void> {
    this.pending.set(true);
    this.roleError.set(null);
    try {
      await this.clubBrowsing.removeMember(this.id(), memberId);
      this.removingUserId.set(null);
      this.members.reload();
    } catch (failure) {
      this.removingUserId.set(null);
      this.roleError.set(serverMessage(failure, 'Could not remove the member.'));
    } finally {
      this.pending.set(false);
    }
  }
}
