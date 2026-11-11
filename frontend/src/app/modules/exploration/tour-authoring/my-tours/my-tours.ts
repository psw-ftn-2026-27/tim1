import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

type Charge = 'Free' | 'Paid';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  protected readonly publishingTourId = signal<string | null>(null);

  private readonly publishModel = signal({ charge: 'Free' as Charge, price: 0 });

  protected readonly publishForm = form(this.publishModel, (path) => {
    validate(path.price, ({ value, valueOf }) =>
      valueOf(path.charge) === 'Paid' && !(value() > 0)
        ? { kind: 'price', message: 'Price must be greater than 0.' }
        : null,
    );
  });

  protected select(tourId: string): void {
    this.error.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected selectForPublishing(tourId: string): void {
    this.error.set(null);
    this.publishingTourId.set(tourId);
    this.publishForm().reset({ charge: 'Free', price: 0 });
  }

  protected closePublishing(): void {
    this.publishingTourId.set(null);
  }

  protected closeTransportTime(): void {
    this.selectedTourId.set(null);
  }

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.publishingTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const paid = this.publishForm.charge().value() === 'Paid';
      await this.tourAuthoring.publish(tourId, {
        price: paid ? this.publishForm.price().value() : 0,
      });
      this.tours.reload();
      this.publishingTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.selectedTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.selectedTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }
}
