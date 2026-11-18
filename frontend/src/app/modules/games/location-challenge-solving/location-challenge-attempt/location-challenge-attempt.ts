import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Map, type MapPin } from '../../../../shared/ui/map/map';
import { ProtectedImage } from '../../../../shared/ui/protected-image/protected-image';
import { LocationChallengeAttemptDto } from '../../api/games-api-types';

@Component({
  imports: [Map, ProtectedImage, RouterLink],
  selector: 'app-location-challenge-attempt',
  styleUrl: './location-challenge-attempt.scss',
  templateUrl: './location-challenge-attempt.html',
})
export class LocationChallengeAttempt {
  readonly id = input.required<string>();

  protected readonly attempt = httpResource<LocationChallengeAttemptDto>(
    () => `/api/games/location-challenges/${this.id()}/attempt`,
  );

  // The shared map cannot draw a circle yet, so it shows the circle's center as a pin and
  // the page states the diameter.
  protected readonly centerPins = computed<MapPin[]>(() =>
    this.attempt.hasValue() ? [this.attempt.value().circle.center] : [],
  );

  protected imageUrl(imageId: string): string {
    return `/api/games/location-challenges/${this.id()}/attempt/images/${imageId}`;
  }

  protected diameter(meters: number): string {
    return `${Math.round(meters)} m`;
  }
}
