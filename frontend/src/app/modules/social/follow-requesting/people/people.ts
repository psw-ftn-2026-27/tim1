import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { UserToFollowDto } from '../../api/social-api-types';
import { FollowRequesting } from '../follow-requesting';
import { UserCard } from '../user-card/user-card';

@Component({
  imports: [FormField, RouterLink, UserCard],
  selector: 'app-people',
  styleUrl: './people.scss',
  templateUrl: './people.html',
})
export class People {
  private readonly followRequesting = inject(FollowRequesting);

  // The list refreshes while the user types, but only after a short pause (300 ms),
  // so the server does not get a request for every key.
  protected readonly form = form(signal({ text: '' }), (path) => {
    debounce(path.text, 300);
  });

  protected readonly users = httpResource<PageResult<UserToFollowDto>>(
    () =>
      `/api/social/follow-requests/search?text=${encodeURIComponent(this.form.text().value())}&page=1&pageSize=20`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async send(recipientId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequesting.send(recipientId);
      this.users.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Request was not sent.'));
    } finally {
      this.pending.set(false);
    }
  }
}
