import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { type MapPin } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeypointDto, TourManagementDto } from '../../api/exploration-api-types';
import { KeypointAuthoring } from '../keypoint-authoring';
import { KeypointForm, KeypointFormValue, createKeypointForm } from '../keypoint-form/keypoint-form';

@Component({
  imports: [KeypointForm, ProtectedImage, RouterLink],
  selector: 'app-edit-keypoint',
  styleUrl: './edit-keypoint.scss',
  templateUrl: './edit-keypoint.html',
})
export class EditKeypoint {
  private readonly keypointAuthoring = inject(KeypointAuthoring);
  private readonly router = inject(Router);

  readonly id = input.required<string>();
  readonly keypointId = input.required<string>();

  protected readonly keypoint = httpResource<KeypointDto>(
    () => `/api/exploration/tours/${this.id()}/keypoints/${this.keypointId()}`,
  );

  protected readonly management = httpResource<TourManagementDto>(
    () => `/api/exploration/tours/${this.id()}/management`,
  );

  protected readonly pins = computed<MapPin[]>(() =>
    this.management.hasValue() ? this.management.value().keypoints.map((k) => k.location) : [],
  );

  protected readonly editedPinIndex = computed(() =>
    this.management.hasValue()
      ? this.management.value().keypoints.findIndex((k) => k.id === this.keypointId())
      : -1,
  );

  protected readonly imageUrl = computed(
    () => `/api/exploration/tours/${this.id()}/keypoints/${this.keypointId()}/image`,
  );

  protected readonly fields = createKeypointForm();
  protected readonly uploadImage = (file: File) => this.keypointAuthoring.uploadImage(file);

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected startEdit(keypoint: KeypointDto): void {
    this.error.set(null);
    this.fields().reset({
      name: keypoint.name,
      description: keypoint.description,
      secret: keypoint.secret ?? '',
      publiclyVisible: keypoint.publiclyVisible,
      location: keypoint.location,
    });
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected async update(value: KeypointFormValue): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.keypointAuthoring.update(this.id(), this.keypointId(), {
        name: value.name,
        description: value.description,
        imageId: value.imageId,
        secret: value.secret,
        location: value.location,
      });
      await this.router.navigate(['/exploration/tour', this.id(), 'manage']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the keypoint.'));
    } finally {
      this.pending.set(false);
    }
  }
}
