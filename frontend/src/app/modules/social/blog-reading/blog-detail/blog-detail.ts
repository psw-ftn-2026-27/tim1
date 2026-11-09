import { httpResource } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
import { BlogReading } from '../blog-reading';

@Component({
  imports: [BlogComments, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);

  // Object URLs of the blog's images, by image id.
  protected readonly imageUrls = signal<Record<string, string>>({});

  protected readonly error = signal<string | null>(null);

  constructor() {
    // Reason: fetching image bytes is an imperative side effect that must run
    // whenever the blog resource gets a value; it has no template trigger.
    effect(() => {
      const blog = this.detail.value();
      if (blog) {
        void this.loadImages(blog);
      }
    });
  }

  private async loadImages(blog: BlogDto): Promise<void> {
    for (const imageId of blog.images) {
      if (this.imageUrls()[imageId]) {
        continue;
      }
      try {
        const blob = await this.blogReading.getPublishedImage(blog.id, imageId);
        const url = URL.createObjectURL(blob);
        this.imageUrls.update((urls) => ({ ...urls, [imageId]: url }));
      } catch {
        // This image stays hidden; the rest of the page still works.
      }
    }
  }

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }
}
