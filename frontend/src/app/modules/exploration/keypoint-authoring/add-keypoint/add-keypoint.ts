import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { type MapPin } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourManagementDto } from '../../api/exploration-api-types';
import { KeypointAuthoring } from '../keypoint-authoring';
import { KeypointForm, KeypointFormValue, createKeypointForm } from '../keypoint-form/keypoint-form';

@Component({
  imports: [KeypointForm, RouterLink],
  selector: 'app-add-keypoint',
  styleUrl: './add-keypoint.scss',
  templateUrl: './add-keypoint.html',
})
export class AddKeypoint {
  private readonly keypointAuthoring = inject(KeypointAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly management = httpResource<TourManagementDto>(
    () => `/api/exploration/tours/${this.id()}/management`,
  );

  protected readonly pins = computed<MapPin[]>(() =>
    this.management.hasValue() ? this.management.value().keypoints.map((k) => k.location) : [],
  );

  protected readonly fields = createKeypointForm();
  protected readonly uploadImage = (file: File) => this.keypointAuthoring.uploadImage(file);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async add(value: KeypointFormValue): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.keypointAuthoring.add(this.id(), {
        name: value.name,
        description: value.description,
        imageId: value.imageId,
        secret: value.secret,
        publiclyVisible: value.publiclyVisible,
        location: value.location,
      });
      await this.router.navigate(['/exploration/tour', this.id(), 'manage']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the keypoint.'));
    } finally {
      this.pending.set(false);
    }
  }
}
