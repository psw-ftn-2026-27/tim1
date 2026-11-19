import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDetailDto, VoteType } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
import { BlogDescription } from '../blog-description/blog-description';
import { BlogReading } from '../blog-reading';

@Component({
  imports: [BlogComments, BlogDescription, ProtectedImage, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDetailDto>(() => `/api/social/blogs/${this.id()}`);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // Index of the image shown large in the gallery.
  protected readonly activeImage = signal(0);

  protected async add(text: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async vote(type: VoteType): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogReading.vote(this.id(), { type });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the vote.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The server sends dates as ISO text; this shows the date in the reader's local time.
  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { dateStyle: 'medium' });
  }

  // A positive score gets a plus sign, a negative one a real minus sign.
  protected formatScore(score: number): string {
    if (score > 0) {
      return `+${score}`;
    }
    return score < 0 ? `−${Math.abs(score)}` : '0';
  }
}
