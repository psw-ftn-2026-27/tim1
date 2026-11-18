import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { SentInvitationDto, UserToInviteDto } from '../../api/social-api-types';
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

  protected readonly form = form(signal({ text: '' }));

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
}
