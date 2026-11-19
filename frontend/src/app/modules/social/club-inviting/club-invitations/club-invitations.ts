import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto, SentInvitationDto, UserToInviteDto } from '../../api/social-api-types';
import { ClubInviting } from '../club-inviting';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-club-invitations',
  styleUrl: './club-invitations.scss',
  templateUrl: './club-invitations.html',
})
export class ClubInvitations {
  private readonly clubInviting = inject(ClubInviting);

  readonly id = input.required<string>();

  protected readonly club = httpResource<ClubDto>(() => `/api/social/clubs/${this.id()}`);

  // The list refreshes while the user types, but only after a short pause (300 ms),
  // so the server does not get a request for every key.
  protected readonly form = form(signal({ text: '' }), (path) => {
    debounce(path.text, 300);
  });

  protected readonly users = httpResource<PageResult<UserToInviteDto>>(
    () =>
      `/api/social/clubs/invitations/search?text=${encodeURIComponent(this.form.text().value())}&page=1&pageSize=20`,
  );

  protected readonly invitations = httpResource<SentInvitationDto[]>(
    () => `/api/social/clubs/${this.id()}/invitations`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async invite(userId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubInviting.invite(this.id(), userId);
      this.invitations.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not send the invitation.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }
}
