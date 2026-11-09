import { httpResource } from '@angular/common/http';
import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [RouterLink],
  selector: 'app-my-blogs',
  styleUrl: './my-blogs.scss',
  templateUrl: './my-blogs.html',
})
export class MyBlogs {
  private readonly blogAuthoring = inject(BlogAuthoring);

  protected readonly blogs = httpResource<BlogDto[]>(() => '/api/social/blogs/mine');

  // Object URLs of the blogs' images, by image id.
  protected readonly imageUrls = signal<Record<string, string>>({});

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    // Reason: fetching image bytes is an imperative side effect that must run
    // whenever the blogs resource gets a value; it has no template trigger.
    effect(() => {
      void this.loadImages(this.blogs.value() ?? []);
    });
  }

  private async loadImages(blogs: BlogDto[]): Promise<void> {
    for (const blog of blogs) {
      for (const imageId of blog.images) {
        if (this.imageUrls()[imageId]) {
          continue;
        }
        try {
          const blob = await this.blogAuthoring.getOwnBlogImage(blog.id, imageId);
          const url = URL.createObjectURL(blob);
          this.imageUrls.update((urls) => ({ ...urls, [imageId]: url }));
        } catch {
          // This image stays hidden; the rest of the page still works.
        }
      }
    }
  }

  protected async publish(blogId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.publish(blogId);
      this.blogs.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
