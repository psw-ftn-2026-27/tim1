import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SaveLocationChallengeDto } from '../../api/games-api-types';
import {
  LocationChallengeForm,
  createLocationChallengeForm,
} from '../location-challenge-form/location-challenge-form';
import { LocationChallengeManagement } from '../location-challenge-management';

@Component({
  imports: [LocationChallengeForm, RouterLink],
  selector: 'app-create-location-challenge',
  styleUrl: './create-location-challenge.scss',
  templateUrl: './create-location-challenge.html',
})
export class CreateLocationChallenge {
  private readonly locationChallengeManagement = inject(LocationChallengeManagement);
  private readonly router = inject(Router);

  protected readonly fields = createLocationChallengeForm();

  // The shared upload component in each point's row runs this command for the chosen photo.
  protected readonly uploadImage = (file: File) =>
    this.locationChallengeManagement.uploadImage(file);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async create(dto: SaveLocationChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.locationChallengeManagement.create(dto);
      await this.router.navigate(['/games/location-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
