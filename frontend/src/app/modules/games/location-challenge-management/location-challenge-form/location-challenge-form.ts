import { Component, computed, input, output, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { ImageUpload } from '../../../../shared/ui/image-upload/image-upload';
import { Map, type MapPin } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { SaveLocationChallengeDto } from '../../api/games-api-types';

// One row of the hidden point list. The location is null until the point is placed on the map,
// and the photo id is null until a photo is uploaded.
export interface HiddenPointFields {
  location: MapPin | null;
  imageId: string | null;
}

export function createLocationChallengeForm() {
  return form(
    signal({
      name: '',
      experiencePoints: null as number | null,
      startLocation: null as MapPin | null,
      hiddenPoints: [] as HiddenPointFields[],
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.experiencePoints, { message: 'Enter the XP.' });
      min(path.experiencePoints, 1, { message: 'XP must be greater than 0.' });
      required(path.startLocation, { message: 'Place the start location on the map.' });
      validate(path.hiddenPoints, ({ value }) => {
        if (value().length === 0) {
          return { kind: 'noPoint', message: 'Add at least one hidden point.' };
        }
        if (value().some((point) => point.location === null)) {
          return { kind: 'pointWithoutLocation', message: 'Place every hidden point on the map.' };
        }
        if (value().some((point) => point.imageId === null)) {
          return { kind: 'pointWithoutPhoto', message: 'Add a photo to every hidden point.' };
        }
        return null;
      });
    },
  );
}

@Component({
  selector: 'app-location-challenge-form',
  imports: [FormField, ImageUpload, Map, ProtectedImage],
  templateUrl: './location-challenge-form.html',
  styleUrl: './location-challenge-form.scss',
})
export class LocationChallengeForm {
  readonly fields = input.required<ReturnType<typeof createLocationChallengeForm>>();
  // The page's command: sends one photo and returns the new image id.
  readonly upload = input.required<(file: File) => Promise<string>>();
  readonly pending = input(false);
  readonly saveLabel = input.required<string>();
  readonly showCancel = input(false);
  readonly save = output<SaveLocationChallengeDto>();
  readonly cancelled = output<void>();

  // The hidden point that the next map click places, by its position in the list;
  // null means the click places the start location.
  protected readonly selectedPoint = signal<number | null>(null);

  // For the hint above the map: what the next click places.
  protected readonly placing = computed(() => {
    const index = this.selectedPoint();
    return index === null ? 'the start location' : `point ${index + 1}`;
  });

  // The map shows the start location and every placed hidden point as pins.
  protected readonly pins = computed<MapPin[]>(() => {
    const { startLocation, hiddenPoints } = this.fields()().value();
    const locations = [startLocation, ...hiddenPoints.map((point) => point.location)];
    return locations.filter((location) => location !== null);
  });

  protected addPoint(): void {
    this.fields()
      .hiddenPoints()
      .value.update((points) => [...points, { location: null, imageId: null }]);
    // The new point is the one the admin places next.
    this.selectedPoint.set(this.fields().hiddenPoints().value().length - 1);
  }

  protected removePoint(index: number): void {
    this.fields()
      .hiddenPoints()
      .value.update((points) => points.filter((_, position) => position !== index));

    // Keep the selection on the same point, whose position moves up when an earlier one goes.
    const selected = this.selectedPoint();
    if (selected === index) {
      this.selectedPoint.set(null);
    } else if (selected !== null && selected > index) {
      this.selectedPoint.set(selected - 1);
    }
  }

  protected place(pin: MapPin): void {
    const index = this.selectedPoint();
    if (index === null) {
      this.fields().startLocation().value.set(pin);
    } else {
      this.fields().hiddenPoints[index]!.location().value.set(pin);
    }
  }

  protected coordinates(pin: MapPin): string {
    return `${pin.latitude.toFixed(5)}, ${pin.longitude.toFixed(5)}`;
  }

  protected submit(event: Event): void {
    event.preventDefault();
    const state = this.fields()();
    if (this.pending() || !state.valid()) {
      return;
    }

    // The form is valid, so the XP, the start location and every point's location and photo are set.
    const { name, experiencePoints, startLocation, hiddenPoints } = state.value();
    this.save.emit({
      name,
      startLocation: startLocation!,
      experiencePoints: experiencePoints!,
      hiddenPoints: hiddenPoints.map((point) => ({
        location: point.location!,
        imageId: point.imageId!,
      })),
    });
  }
}
