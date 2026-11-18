import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async archive(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.archive(tourId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not archive the tour.'));
    } finally {
      this.tours.reload();
      this.pending.set(false);
    }
  }

  protected async reactivate(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.reactivate(tourId);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reactivate the tour.'));
    } finally {
      this.tours.reload();
      this.pending.set(false);
    }
  }
}
