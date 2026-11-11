import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { AuthorToFollowDto } from '../../api/social-api-types';
import { AuthorCard } from '../author-card/author-card';
import { FollowRequesting } from '../follow-requesting';

@Component({
  imports: [AuthorCard, RouterLink],
  selector: 'app-people',
  styleUrl: './people.scss',
  templateUrl: './people.html',
})
export class People {
  private readonly followRequesting = inject(FollowRequesting);

  protected readonly authors = httpResource<PageResult<AuthorToFollowDto>>(
    () => '/api/social/follow-requests/authors?page=1&pageSize=20',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async send(recipientId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.followRequesting.send(recipientId);
      this.authors.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Zahtev nije poslat.'));
    } finally {
      this.pending.set(false);
    }
  }
}
