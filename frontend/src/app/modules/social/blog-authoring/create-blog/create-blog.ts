import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogAuthoring } from '../blog-authoring';

const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

// Client-only: one entry of the "Slike" section.
export interface ImageItem {
  key: number;
  name: string;
  state: 'sending' | 'ready' | 'rejected';
  imageId: string | null;
  previewUrl: string | null;
  message: string | null;
}

@Component({
  imports: [FormField],
  selector: 'app-create-blog',
  styleUrl: './create-blog.scss',
  templateUrl: './create-blog.html',
})
export class CreateBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly images = signal<ImageItem[]>([]);
  protected readonly anySending = computed(() => this.images().some((i) => i.state === 'sending'));
  private nextKey = 0;

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.create({
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: this.images()
          .filter((i) => i.state === 'ready')
          .map((i) => i.imageId as string),
      });
      await this.router.navigate(['/social/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the blog.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async onFilesChosen(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';

    // First show every file in the list, then send the valid ones one by one.
    const toSend: { key: number; file: File }[] = [];
    for (const file of files) {
      const key = this.nextKey++;
      const message = this.checkFile(file);
      this.images.update((list) => [
        ...list,
        {
          key,
          name: file.name,
          state: message ? 'rejected' : 'sending',
          imageId: null,
          previewUrl: null,
          message,
        },
      ]);
      if (!message) {
        toSend.push({ key, file });
      }
    }
    for (const item of toSend) {
      await this.send(item.key, item.file);
    }
  }

  protected remove(key: number): void {
    const item = this.images().find((i) => i.key === key);
    if (item?.previewUrl) {
      URL.revokeObjectURL(item.previewUrl);
    }
    this.images.update((list) => list.filter((i) => i.key !== key));
  }

  private checkFile(file: File): string | null {
    if (!IMAGE_TYPES.includes(file.type)) {
      return 'Dozvoljeni su samo JPEG, PNG i WebP.';
    }
    if (file.size > MAX_IMAGE_BYTES) {
      return 'Slika je veća od 5 MB.';
    }
    return null;
  }

  private async send(key: number, file: File): Promise<void> {
    try {
      const imageId = await this.blogAuthoring.uploadImage(file);
      const blob = await this.blogAuthoring.getUnattachedImage(imageId);
      const previewUrl = URL.createObjectURL(blob);
      this.change(key, { state: 'ready', imageId, previewUrl });
    } catch (failure) {
      this.change(key, {
        state: 'rejected',
        message: serverMessage(failure, 'Slika nije poslata.'),
      });
    }
  }

  private change(key: number, changes: Partial<ImageItem>): void {
    this.images.update((list) => list.map((i) => (i.key === key ? { ...i, ...changes } : i)));
  }
}
