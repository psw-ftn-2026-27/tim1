import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { type MapPin } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeypointDto } from '../../api/exploration-api-types';
import { KeypointAuthoring } from '../keypoint-authoring';
import {
  KeypointForm,
  KeypointFormModel,
  KeypointFormValue,
  createKeypointForm,
  emptyKeypointModel,
} from '../keypoint-form/keypoint-form';

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

  protected readonly keypoints = httpResource<KeypointDto[]>(
    () => `/api/exploration/tours/${this.id()}/keypoints`,
  );

  protected readonly pins = computed<MapPin[]>(() =>
    this.keypoints.hasValue() ? this.keypoints.value().map((k) => k.location) : [],
  );

  protected readonly editedPinIndex = computed(() =>
    this.keypoints.hasValue()
      ? this.keypoints.value().findIndex((k) => k.id === this.keypointId())
      : -1,
  );

  protected readonly imageUrl = computed(
    () => `/api/exploration/tours/${this.id()}/keypoints/${this.keypointId()}/image`,
  );

  // The form starts with the loaded keypoint: the model is recomputed from the resource when it arrives.
  private readonly model = linkedSignal<KeypointFormModel>(() => {
    const loaded = this.keypoint.hasValue() ? this.keypoint.value() : null;
    if (loaded === null) {
      return emptyKeypointModel();
    }
    return {
      name: loaded.name,
      description: loaded.description,
      secret: loaded.secret ?? '',
      publiclyVisible: loaded.publiclyVisible,
      location: loaded.location,
    };
  });

  protected readonly fields = createKeypointForm(this.model);
  protected readonly uploadImage = (file: File) => this.keypointAuthoring.uploadImage(file);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

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
        lengthKm: value.lengthKm,
      });
      await this.router.navigate(['/exploration/tour', this.id(), 'edit']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the keypoint.'));
    } finally {
      this.pending.set(false);
    }
  }
}
