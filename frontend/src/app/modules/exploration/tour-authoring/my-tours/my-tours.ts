import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Analytics } from '../../../../core/analytics/analytics';
import { formatLength } from '../../../../shared/util/format-length';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

type Charge = 'Free' | 'Paid';

// The section open under one row of the table: publishing the tour or adding a travel time.
interface OpenPanel {
  tourId: string;
  kind: 'publish' | 'time';
}

const TRANSPORTS: TransportMode[] = ['Walking', 'Bicycle', 'Car'];

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly analytics = inject(Analytics);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly formatLength = formatLength;

  protected readonly pending = signal(false);
  // A failed Archive or Reactivate, shown above the table.
  protected readonly error = signal<string | null>(null);
  // A failed Publish or Add time, shown at the top of the open section.
  protected readonly panelError = signal<string | null>(null);

  protected readonly panel = signal<OpenPanel | null>(null);

  protected readonly publishForm = form(
    signal({ charge: 'Free' as Charge, price: 0 }),
    (path) => {
      validate(path.price, ({ value, valueOf }) =>
        valueOf(path.charge) === 'Paid' && !(value() > 0)
          ? { kind: 'price', message: 'A paid tour must cost more than 0 RSD.' }
          : null,
      );
    },
  );

  protected readonly timeForm = form(
    signal({ transport: 'Walking' as TransportMode, minutes: 30 }),
    (path) => {
      required(path.minutes, { message: 'Minutes are required.' });
      min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
    },
  );

  // The transport modes the tour in the open section has no time for yet.
  protected readonly freeTransports = computed(() => {
    const open = this.panel();
    const tour = this.tours.value()?.find((t) => t.id === open?.tourId);
    return tour === undefined ? [] : this.transportsWithoutTime(tour);
  });

  protected transportsWithoutTime(tour: TourDto): TransportMode[] {
    const taken = tour.transportTimes.map((time) => time.transport);
    return TRANSPORTS.filter((transport) => !taken.includes(transport));
  }

  protected isOpen(tourId: string, kind: OpenPanel['kind']): boolean {
    const open = this.panel();
    return open?.tourId === tourId && open.kind === kind;
  }

  protected openPublish(tourId: string): void {
    this.publishForm().reset({ charge: 'Free', price: 0 });
    this.openPanel({ tourId, kind: 'publish' });
  }

  protected openTime(tour: TourDto): void {
    this.timeForm().reset({ transport: this.transportsWithoutTime(tour)[0] ?? 'Walking', minutes: 30 });
    this.openPanel({ tourId: tour.id, kind: 'time' });
  }

  protected closePanel(): void {
    this.panel.set(null);
    this.panelError.set(null);
  }

  protected async publish(event: Event, tour: TourDto): Promise<void> {
    event.preventDefault();
    // The button stays active, so a wrong price shows its message instead of being sent.
    if (!this.publishForm().valid()) {
      this.publishForm.price().markAsTouched();
      return;
    }
    const paid = this.publishForm.charge().value() === 'Paid';
    const price = paid ? this.publishForm.price().value() : 0;
    await this.runInPanel(async () => {
      await this.tourAuthoring.publish(tour.id, { price });
      this.analytics.track('tour_published', { tour_id: tour.id, difficulty: tour.difficulty });
    }, 'Could not publish the tour.');
  }

  protected async addTime(event: Event, tourId: string): Promise<void> {
    event.preventDefault();
    const transport = this.timeForm.transport().value();
    const minutes = this.timeForm.minutes().value();
    await this.runInPanel(
      () => this.tourAuthoring.addTransportTime(tourId, { transport, minutes }),
      'Could not add the travel time.',
    );
  }

  protected async archive(tourId: string): Promise<void> {
    await this.run(() => this.tourAuthoring.archive(tourId), 'Could not archive the tour.');
  }

  protected async reactivate(tourId: string): Promise<void> {
    await this.run(() => this.tourAuthoring.reactivate(tourId), 'Could not reactivate the tour.');
  }

  private openPanel(panel: OpenPanel): void {
    this.panelError.set(null);
    this.panel.set(panel);
  }

  // A command from the open section: on success the section closes; on failure it stays open
  // with the server's message. The list is reloaded either way.
  private async runInPanel(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.panelError.set(null);
    try {
      await command();
      this.panel.set(null);
    } catch (failure) {
      this.panelError.set(serverMessage(failure, fallback));
    } finally {
      this.tours.reload();
      this.pending.set(false);
    }
  }

  private async run(command: () => Promise<void>, fallback: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.closePanel();
    try {
      await command();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.tours.reload();
      this.pending.set(false);
    }
  }
}
