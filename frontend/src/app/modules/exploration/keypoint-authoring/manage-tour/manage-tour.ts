import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Map, type MapPin, type MapRouteMode } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeypointDto, TourManagementDto } from '../../api/exploration-api-types';
import { KeypointAuthoring } from '../keypoint-authoring';

@Component({
  imports: [FormField, Map, RouterLink],
  selector: 'app-manage-tour',
  styleUrl: './manage-tour.scss',
  templateUrl: './manage-tour.html',
})
export class ManageTour {
  private readonly keypointAuthoring = inject(KeypointAuthoring);

  readonly id = input.required<string>();

  protected readonly management = httpResource<TourManagementDto>(
    () => `/api/exploration/tours/${this.id()}/management`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly routeForm = form(signal({ mode: 'Walking' as MapRouteMode }));

  protected readonly pins = computed<MapPin[]>(() =>
    this.management.hasValue() ? this.management.value().keypoints.map((k) => k.location) : [],
  );

  protected readonly routeRequest = computed(() => ({
    points: this.pins(),
    mode: this.routeForm.mode().value(),
  }));

  protected async remove(keypoint: KeypointDto): Promise<void> {
    await this.run(
      () => this.keypointAuthoring.remove(this.id(), keypoint.id),
      'Could not remove the keypoint.',
    );
  }

  protected async setVisibility(keypoint: KeypointDto): Promise<void> {
    await this.run(
      () => this.keypointAuthoring.setVisibility(this.id(), keypoint.id, !keypoint.publiclyVisible),
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
      this.management.reload();
      this.pending.set(false);
    }
  }
}
