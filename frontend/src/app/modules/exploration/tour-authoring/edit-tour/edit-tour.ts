import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Map, type MapPin, type MapRouteMode } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { formatLength } from '../../../../shared/util/format-length';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeypointDto, TourDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

// A tour needs at least this many keypoints to be published (the server checks it on Publish).
const KEYPOINTS_TO_PUBLISH = 2;

@Component({
  imports: [FormField, Map, ProtectedImage, RouterLink],
  selector: 'app-edit-tour',
  styleUrl: './edit-tour.scss',
  templateUrl: './edit-tour.html',
})
export class EditTour {
  protected readonly modes: MapRouteMode[] = ['Walking', 'Bicycle', 'Car'];
  protected readonly keypointsToPublish = KEYPOINTS_TO_PUBLISH;
  protected readonly formatLength = formatLength;

  private readonly tourAuthoring = inject(TourAuthoring);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDto>(() => `/api/exploration/tours/mine/${this.id()}`);

  protected readonly keypoints = httpResource<KeypointDto[]>(
    () => `/api/exploration/tours/${this.id()}/keypoints`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The route mode only changes the line on the map; the saved length is always the walking one.
  protected readonly routeForm = form(signal({ mode: 'Walking' as MapRouteMode }));

  protected readonly pins = computed<MapPin[]>(() =>
    this.keypoints.hasValue() ? this.keypoints.value().map((k) => k.location) : [],
  );

  protected readonly routeRequest = computed(() => ({
    points: this.pins(),
    mode: this.routeForm.mode().value(),
  }));

  protected imageUrl(keypoint: KeypointDto): string {
    return `/api/exploration/tours/${this.id()}/keypoints/${keypoint.id}/image`;
  }

  // After a removal the page waits for the map to measure the remaining walking route; this is
  // the number of keypoints that route has. Null means nothing is waiting.
  private readonly pinCountAwaitingLength = signal<number | null>(null);

  protected async remove(keypoint: KeypointDto): Promise<void> {
    const remaining = this.pins().length - 1;
    await this.run(async () => {
      await this.tourAuthoring.removeKeypoint(this.id(), keypoint.id);
      if (remaining < 2) {
        // Fewer than two keypoints have no route, so there is nothing to measure.
        await this.tourAuthoring.setLength(this.id(), { lengthKm: 0 });
      } else {
        this.routeForm.mode().value.set('Walking');
        this.pinCountAwaitingLength.set(remaining);
      }
    }, 'Could not remove the keypoint.');
  }

  // The map reports a length every time its route changes. Only the first walking length
  // measured for the remaining keypoints after a removal is sent to the server.
  protected async saveMeasuredLength(lengthKm: number | null): Promise<void> {
    const waitingFor = this.pinCountAwaitingLength();
    if (
      lengthKm === null ||
      waitingFor === null ||
      this.pins().length !== waitingFor ||
      this.routeForm.mode().value() !== 'Walking'
    ) {
      return;
    }
    this.pinCountAwaitingLength.set(null);
    await this.run(
      () => this.tourAuthoring.setLength(this.id(), { lengthKm }),
      'Could not save the tour length.',
    );
  }

  protected async setVisibility(keypoint: KeypointDto): Promise<void> {
    await this.run(
      () =>
        this.tourAuthoring.setKeypointVisibility(this.id(), keypoint.id, !keypoint.publiclyVisible),
      'Could not change the visibility.',
    );
  }

  private async run(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await command();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.tour.reload();
      this.keypoints.reload();
      this.pending.set(false);
    }
  }
}
