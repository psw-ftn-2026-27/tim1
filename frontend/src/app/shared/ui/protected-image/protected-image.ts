import { httpResource } from '@angular/common/http';
import { Component, effect, input, signal } from '@angular/core';

// Shows an image whose address requires sign-in. A plain <img src> sends no token, so the
// component reads the image through HttpClient (the auth interceptor adds the token) and
// shows it through an object URL. This is the documented exception in frontend/AGENTS.md:
// a shared component in shared/ui/ that reads protected content may fetch it itself.
@Component({
  selector: 'app-protected-image',
  styleUrl: './protected-image.scss',
  templateUrl: './protected-image.html',
})
export class ProtectedImage {
  readonly src = input.required<string>();

  private readonly image = httpResource.blob(() => this.src());

  protected readonly url = signal<string | null>(null);

  constructor() {
    // Reason: an object URL is a browser resource outside the component tree; it is created
    // when the image arrives and revoked when the image changes or the component is destroyed.
    effect((onCleanup) => {
      // An image that cannot be read has no value; it shows nothing and the page goes on.
      if (!this.image.hasValue()) {
        this.url.set(null);
        return;
      }
      const url = URL.createObjectURL(this.image.value());
      this.url.set(url);
      onCleanup(() => URL.revokeObjectURL(url));
    });
  }
}
