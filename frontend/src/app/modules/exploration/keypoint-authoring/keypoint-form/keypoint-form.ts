import { Component, WritableSignal, computed, input, output, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { ImageUpload } from '../../../../shared/ui/image-upload/image-upload';
import { Map, type MapPin, type MapRouteMode } from '../../../../shared/ui/map/map';

export interface KeypointFormValue {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  publiclyVisible: boolean;
  location: MapPin;
}

// What the form edits. The page owns it, so the page decides what the form starts with.
export interface KeypointFormModel {
  name: string;
  description: string;
  secret: string;
  publiclyVisible: boolean;
  location: MapPin | null;
}

export function emptyKeypointModel(): KeypointFormModel {
  return { name: '', description: '', secret: '', publiclyVisible: false, location: null };
}

export function createKeypointForm(model: WritableSignal<KeypointFormModel>) {
  return form(model, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.description, { message: 'Description is required.' });
    validate(path.location, ({ value }) =>
      value() === null ? { kind: 'location', message: 'Choose a location on the map.' } : null,
    );
  });
}

@Component({
  imports: [FormField, ImageUpload, Map],
  selector: 'app-keypoint-form',
  styleUrl: './keypoint-form.scss',
  templateUrl: './keypoint-form.html',
})
export class KeypointForm {
  readonly fields = input.required<ReturnType<typeof createKeypointForm>>();
  // The tour's existing keypoints, shown as pins with a route between them.
  readonly pins = input.required<readonly MapPin[]>();
  // Edit only: the position of the edited keypoint among the pins, so its old pin is replaced.
  readonly editedPinIndex = input<number | null>(null);
  // The page's command: sends one image file and returns its id.
  readonly upload = input.required<(file: File) => Promise<string>>();
  // The image the keypoint already has (edit), kept unless a new one is uploaded.
  readonly currentImageId = input<string | null>(null);
  readonly showVisibility = input(false);
  readonly pending = input(false);
  readonly saveLabel = input.required<string>();
  readonly save = output<KeypointFormValue>();

  protected readonly imageIds = signal<string[]>([]);
  protected readonly imagesSending = signal(false);
  protected readonly modes: MapRouteMode[] = ['Walking', 'Bicycle', 'Car'];

  protected readonly routeForm = form(signal({ mode: 'Walking' as MapRouteMode }));

  // The pins the map shows: the tour's keypoints with the chosen location in them. On add the
  // chosen location is the new last pin; on edit it takes the place of the edited keypoint's pin.
  protected readonly mapPins = computed<readonly MapPin[]>(() => {
    const location = this.fields().location().value();
    const index = this.editedPinIndex();
    if (location === null) {
      return this.pins();
    }
    if (index === null) {
      return [...this.pins(), location];
    }
    return this.pins().map((pin, position) => (position === index ? location : pin));
  });

  protected readonly routeRequest = computed(() => ({
    points: this.mapPins(),
    mode: this.routeForm.mode().value(),
  }));

  // The one image the keypoint will have: the uploaded one, or else the current one.
  protected readonly imageId = computed<string | null>(() => {
    const uploaded = this.imageIds();
    if (uploaded.length === 1) {
      return uploaded[0]!;
    }
    return uploaded.length === 0 ? this.currentImageId() : null;
  });

  protected readonly imageError = computed<string | null>(() => {
    if (this.imageIds().length > 1) {
      return 'Choose exactly one image.';
    }
    return this.imageId() === null ? 'An image is required.' : null;
  });

  protected pickLocation(pin: MapPin): void {
    this.fields().location().value.set(pin);
  }

  protected submit(event: Event): void {
    event.preventDefault();
    const state = this.fields()();
    const imageId = this.imageId();
    const { name, description, secret, publiclyVisible, location } = state.value();
    if (this.pending() || !state.valid() || imageId === null || location === null) {
      return;
    }
    this.save.emit({
      name,
      description,
      imageId,
      secret: secret.trim() === '' ? null : secret,
      publiclyVisible,
      location,
    });
  }
}
