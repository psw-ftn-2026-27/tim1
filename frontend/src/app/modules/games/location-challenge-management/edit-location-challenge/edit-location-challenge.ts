import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Map, type MapPin } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeDto, SaveLocationChallengeDto } from '../../api/games-api-types';
import {
  LocationChallengeForm,
  createLocationChallengeForm,
} from '../location-challenge-form/location-challenge-form';
import { LocationChallengeManagement } from '../location-challenge-management';

@Component({
  imports: [LocationChallengeForm, Map, ProtectedImage, RouterLink],
  selector: 'app-edit-location-challenge',
  styleUrl: './edit-location-challenge.scss',
  templateUrl: './edit-location-challenge.html',
})
export class EditLocationChallenge {
  private readonly locationChallengeManagement = inject(LocationChallengeManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<LocationChallengeDto>(
    () => `/api/games/location-challenges/${this.id()}`,
  );

  // The read-only map shows the start location and every hidden point as pins.
  protected readonly challengePins = computed<MapPin[]>(() =>
    this.challenge.hasValue()
      ? [
          this.challenge.value().startLocation,
          ...this.challenge.value().hiddenPoints.map((point) => point.location),
        ]
      : [],
  );

  protected readonly fields = createLocationChallengeForm();

  // The shared upload component in each point's row runs this command for the chosen photo.
  protected readonly uploadImage = (file: File) =>
    this.locationChallengeManagement.uploadImage(file);

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected coordinates(pin: MapPin): string {
    return `${pin.latitude.toFixed(5)}, ${pin.longitude.toFixed(5)}`;
  }

  protected startEdit(challenge: LocationChallengeDto): void {
    this.error.set(null);
    this.fields().reset({
      name: challenge.name,
      experiencePoints: challenge.experiencePoints,
      startLocation: challenge.startLocation,
      hiddenPoints: challenge.hiddenPoints.map((point) => ({
        location: point.location,
        imageId: point.imageId,
      })),
    });
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected async update(dto: SaveLocationChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.update(this.id(), dto);
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
