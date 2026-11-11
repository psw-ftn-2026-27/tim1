import { Component, OnDestroy, input, output, signal } from '@angular/core';
import { serverMessage } from '../../util/server-message';

// The same limits the server checks (Shared.Infrastructure ImageProcessor). The client
// checks only size and type, and only for a faster answer; the server checks everything again.
const MAX_IMAGE_BYTES = 5 * 1024 * 1024;
const IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

// One entry of the list. View state only, never sent to the server.
interface ImageItem {
  key: number;
  name: string;
  state: 'sending' | 'ready' | 'rejected';
  imageId: string | null;
  previewUrl: string | null;
  message: string | null;
}

@Component({
  selector: 'app-image-upload',
  styleUrl: './image-upload.scss',
  templateUrl: './image-upload.html',
})
export class ImageUpload implements OnDestroy {
  // The page's command: sends one file and returns the new image id.
  readonly upload = input.required<(file: File) => Promise<string>>();

  readonly imageIds = output<string[]>();
  readonly sending = output<boolean>();

  protected readonly accept = IMAGE_TYPES.join(',');
  protected readonly items = signal<ImageItem[]>([]);
  private nextKey = 0;

  protected async choose(event: Event): Promise<void> {
    const fileInput = event.target as HTMLInputElement;
    const files = Array.from(fileInput.files ?? []);
    fileInput.value = '';

    // First show every chosen file in the list, then send the valid ones one by one.
    const toSend: { key: number; file: File }[] = [];
    for (const file of files) {
      const key = this.nextKey++;
      const message = this.check(file);
      this.add({
        key,
        name: file.name,
        state: message === null ? 'sending' : 'rejected',
        imageId: null,
        previewUrl: message === null ? URL.createObjectURL(file) : null,
        message,
      });
      if (message === null) {
        toSend.push({ key, file });
      }
    }
    for (const { key, file } of toSend) {
      await this.send(key, file);
    }
  }

  protected remove(key: number): void {
    const item = this.items().find((i) => i.key === key);
    this.revoke(item?.previewUrl ?? null);
    this.setItems(this.items().filter((i) => i.key !== key));
  }

  ngOnDestroy(): void {
    for (const item of this.items()) {
      this.revoke(item.previewUrl);
    }
  }

  private check(file: File): string | null {
    if (!IMAGE_TYPES.includes(file.type)) {
      return 'Only JPEG, PNG and WebP images are accepted.';
    }
    if (file.size > MAX_IMAGE_BYTES) {
      return 'The image is larger than 5 MB.';
    }
    return null;
  }

  private async send(key: number, file: File): Promise<void> {
    try {
      const imageId = await this.upload()(file);
      this.change(key, { state: 'ready', imageId });
    } catch (failure) {
      // A rejected file may not be a readable image, so its preview goes away.
      this.revoke(this.items().find((i) => i.key === key)?.previewUrl ?? null);
      this.change(key, {
        state: 'rejected',
        previewUrl: null,
        message: serverMessage(failure, 'The image could not be uploaded.'),
      });
    }
  }

  private add(item: ImageItem): void {
    this.setItems([...this.items(), item]);
  }

  private change(key: number, changes: Partial<ImageItem>): void {
    this.setItems(this.items().map((i) => (i.key === key ? { ...i, ...changes } : i)));
  }

  // Every change of the list goes through here, so the page always hears the current state.
  private setItems(items: ImageItem[]): void {
    this.items.set(items);
    this.imageIds.emit(items.filter((i) => i.state === 'ready').map((i) => i.imageId as string));
    this.sending.emit(items.some((i) => i.state === 'sending'));
  }

  private revoke(url: string | null): void {
    if (url !== null) {
      URL.revokeObjectURL(url);
    }
  }
}
