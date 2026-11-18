import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationDto, NearbyLocationChallengesDto } from '../../api/games-api-types';
import { LocationChallengeSolving } from '../location-challenge-solving';

@Component({
  imports: [RouterLink],
  selector: 'app-nearby-location-challenges',
  styleUrl: './nearby-location-challenges.scss',
  templateUrl: './nearby-location-challenges.html',
})
export class NearbyLocationChallenges {
  private readonly locationChallengeSolving = inject(LocationChallengeSolving);
  private readonly router = inject(Router);

  // A fresh resource on every visit, so coming back from setting the position reloads it.
  protected readonly nearby = httpResource<NearbyLocationChallengesDto>(
    () => '/api/games/location-challenges/nearby',
  );

  protected readonly activatingId = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);

  protected coordinates(location: LocationDto): string {
    return `${location.latitude.toFixed(5)}, ${location.longitude.toFixed(5)}`;
  }

  protected distance(meters: number): string {
    return meters < 1000 ? `${Math.round(meters)} m` : `${(meters / 1000).toFixed(1)} km`;
  }

  protected async activate(challengeId: string): Promise<void> {
    this.activatingId.set(challengeId);
    this.error.set(null);
    try {
      await this.locationChallengeSolving.activate(challengeId);
      await this.router.navigate(['/games/location-challenges', challengeId, 'attempt']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the challenge.'));
      this.nearby.reload();
    } finally {
      this.activatingId.set(null);
    }
  }
}
