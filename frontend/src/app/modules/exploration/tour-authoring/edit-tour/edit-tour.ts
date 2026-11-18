import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Analytics } from '../../../../core/analytics/analytics';
import { Map, type MapPin, type MapRouteMode } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeypointDto, TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

type Charge = 'Free' | 'Paid';

@Component({
  imports: [FormField, Map, RouterLink],
  selector: 'app-edit-tour',
  styleUrl: './edit-tour.scss',
  templateUrl: './edit-tour.html',
})
export class EditTour {
  protected readonly modes: MapRouteMode[] = ['Walking', 'Bicycle', 'Car'];

  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly analytics = inject(Analytics);

  readonly id = input.required<string>();

  protected readonly tour = httpResource<TourDto>(() => `/api/exploration/tours/mine/${this.id()}`);

  protected readonly keypoints = httpResource<KeypointDto[]>(
    () => `/api/exploration/tours/${this.id()}/keypoints`,
  );

  protected readonly transports: TransportMode[] = ['Walking', 'Bicycle', 'Car'];

  private readonly timeModel = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly timeForm = form(this.timeModel, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  protected readonly availableTransports = computed(() => {
    if (!this.tour.hasValue()) {
      return [];
    }
    const taken = this.tour.value().transportTimes.map((time) => time.transport);
    return this.transports.filter((transport) => !taken.includes(transport));
  });

  protected readonly selectedTransport = computed(() => {
    const available = this.availableTransports();
    const chosen = this.timeForm.transport().value();
    return available.includes(chosen) ? chosen : available[0];
  });

  protected readonly timeAdded = signal<string | null>(null);

  protected readonly publishing = signal(false);

  private readonly publishModel = signal({ charge: 'Free' as Charge, price: 0 });

  protected readonly publishForm = form(this.publishModel, (path) => {
    validate(path.price, ({ value, valueOf }) =>
      valueOf(path.charge) === 'Paid' && !(value() > 0)
        ? { kind: 'price', message: 'Price must be greater than 0.' }
        : null,
    );
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly routeForm = form(signal({ mode: 'Walking' as MapRouteMode }));

  protected readonly pins = computed<MapPin[]>(() =>
    this.keypoints.hasValue() ? this.keypoints.value().map((k) => k.location) : [],
  );

  protected readonly routeRequest = computed(() => ({
    points: this.pins(),
    mode: this.routeForm.mode().value(),
  }));

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

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const transport = this.selectedTransport();
    const minutes = this.timeForm.minutes().value();
    await this.run(async () => {
      await this.tourAuthoring.addTransportTime(this.id(), { transport, minutes });
      this.timeAdded.set(`${transport} · ${minutes} min`);
    }, 'Could not add the transport time.');
  }

  protected openPublishing(): void {
    this.publishForm().reset({ charge: 'Free', price: 0 });
    this.publishing.set(true);
  }

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    const paid = this.publishForm.charge().value() === 'Paid';
    const price = paid ? this.publishForm.price().value() : 0;
    await this.run(async () => {
      await this.tourAuthoring.publish(this.id(), { price });
      this.analytics.track('tour_published', {
        tour_id: this.id(),
        difficulty: this.tour.hasValue() ? this.tour.value().difficulty : 'unknown',
      });
      this.publishing.set(false);
    }, 'Could not publish the tour.');
  }

  private async run(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.timeAdded.set(null);
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
