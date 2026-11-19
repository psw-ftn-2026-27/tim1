import { Component, WritableSignal, computed, input, output, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/ui/image-upload/image-upload';
import { Map, type MapPin, type MapRouteMode } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';

export interface KeypointFormValue {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  publiclyVisible: boolean;
  location: MapPin;
  // The walking route through all the tour's keypoints, including this one, measured by the map.
  lengthKm: number;
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
      value() === null ? { kind: 'location', message: 'Click the map to place the pin.' } : null,
    );
  });
}

@Component({
  imports: [FormField, ImageUpload, Map, ProtectedImage, RouterLink],
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
  // Edit only: the address of the current image, shown in the image field.
  readonly currentImageUrl = input<string | null>(null);
  readonly showVisibility = input(false);
  readonly pending = input(false);
  readonly saveLabel = input.required<string>();
  // Where Cancel goes, as a routerLink value.
  readonly cancelLink = input.required<string[]>();
  readonly save = output<KeypointFormValue>();

  protected readonly modes: MapRouteMode[] = ['Walking', 'Bicycle', 'Car'];

  protected readonly imageIds = signal<string[]>([]);
  protected readonly imagesSending = signal(false);
  // True after the first Save, so the "image is required" message waits until then.
  protected readonly submitted = signal(false);
  // The last walking length the map measured; null while it is being measured or cannot be found.
  protected readonly routeLengthKm = signal<number | null>(null);

  // The route mode only changes the line on the map. The tour length is always the walking route.
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

  // A single point has no route, so the tour is 0 km long; otherwise wait for the map.
  protected readonly lengthKm = computed<number | null>(() =>
    this.mapPins().length < 2 ? 0 : this.routeLengthKm(),
  );

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

  // The map measures the route of the mode on screen; only a walking length is kept.
  protected measured(lengthKm: number | null): void {
    if (this.routeForm.mode().value() === 'Walking') {
      this.routeLengthKm.set(lengthKm);
    }
  }

  // A new location changes the tour length, so the map goes back to the walking route to measure it.
  protected pickLocation(pin: MapPin): void {
    this.routeForm.mode().value.set('Walking');
    this.fields().location().value.set(pin);
  }

  protected submit(event: Event): void {
    event.preventDefault();
    this.submitted.set(true);
    const state = this.fields()();
    const imageId = this.imageId();
    const { name, description, secret, publiclyVisible, location } = state.value();
    const lengthKm = this.lengthKm();
    if (!state.valid()) {
      this.fields().name().markAsTouched();
      this.fields().description().markAsTouched();
      this.fields().location().markAsTouched();
    }
    if (this.pending() || !state.valid() || imageId === null || location === null || lengthKm === null) {
      return;
    }
    this.save.emit({
      name,
      description,
      imageId,
      secret: secret.trim() === '' ? null : secret,
      publiclyVisible,
      location,
      lengthKm,
    });
  }
}
