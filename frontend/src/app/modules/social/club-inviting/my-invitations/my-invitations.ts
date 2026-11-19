import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { MyInvitationDto } from '../../api/social-api-types';
import { ClubInviting } from '../club-inviting';

@Component({
  imports: [RouterLink],
  selector: 'app-my-invitations',
  styleUrl: './my-invitations.scss',
  templateUrl: './my-invitations.html',
})
export class MyInvitations {
  private readonly clubInviting = inject(ClubInviting);

  protected readonly invitations = httpResource<MyInvitationDto[]>(
    () => '/api/social/clubs/invitations/mine',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async accept(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubInviting.accept(clubId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not accept the invitation.'));
    } finally {
      this.pending.set(false);
      this.invitations.reload();
    }
  }

  protected async reject(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubInviting.decline(clubId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reject the invitation.'));
    } finally {
      this.pending.set(false);
      this.invitations.reload();
    }
  }

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }
}
