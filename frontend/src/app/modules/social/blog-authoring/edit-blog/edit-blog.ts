import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImageUpload } from '../../../../shared/ui/image-upload/image-upload';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [FormField, ImageUpload, ProtectedImage],
  selector: 'app-edit-blog',
  styleUrl: './edit-blog.scss',
  templateUrl: './edit-blog.html',
})
export class EditBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly blog = httpResource<BlogDto>(() => `/api/social/blogs/mine/${this.id()}`);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly editing = signal(false);

  // Existing images the author keeps; "Remove" takes one out of this list.
  protected readonly keptImageIds = signal<string[]>([]);

  // The shared upload component runs this command for each chosen file.
  protected readonly uploadImage = (file: File) => this.blogAuthoring.uploadImage(file);
  protected readonly newImageIds = signal<string[]>([]);
  protected readonly imagesSending = signal(false);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected startEdit(blog: BlogDto): void {
    this.error.set(null);
    this.form().reset({ title: blog.title, description: blog.description });
    this.keptImageIds.set([...blog.images]);
    this.editing.set(true);
  }

  protected removeImage(imageId: string): void {
    this.keptImageIds.update((ids) => ids.filter((id) => id !== imageId));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.update(this.id(), {
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: [...this.keptImageIds(), ...this.newImageIds()],
      });
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
