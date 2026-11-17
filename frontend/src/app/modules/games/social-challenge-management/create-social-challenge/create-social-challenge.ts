import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Map } from '../../../../shared/ui/map/map';
import type { MapPin } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField, Map, RouterLink],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly socialChallengeManagement = inject(SocialChallengeManagement);
  private readonly router = inject(Router);

  // The times are "HH:mm", exactly as an <input type="time"> holds them.
  // A number is null while its input is empty; the location is null until a point is picked.
  protected readonly form = form(
    signal({
      name: '',
      activeFrom: '',
      activeUntil: '',
      requiredParticipants: null as number | null,
      radiusMeters: null as number | null,
      experiencePoints: null as number | null,
      completionMessage: '',
      latitude: null as number | null,
      longitude: null as number | null,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.activeFrom, { message: 'Start time is required.' });
      required(path.activeUntil, { message: 'End time is required.' });
      required(path.requiredParticipants, { message: 'Enter how many people are needed.' });
      min(path.requiredParticipants, 2, { message: 'At least 2 people are needed.' });
      required(path.radiusMeters, { message: 'Enter the range in meters.' });
      min(path.radiusMeters, 1, { message: 'Range must be greater than 0.' });
      required(path.experiencePoints, { message: 'Enter the XP.' });
      min(path.experiencePoints, 1, { message: 'XP must be greater than 0.' });
      required(path.completionMessage, { message: 'Exit message is required.' });
      required(path.latitude, { message: 'Pick the location on the map.' });
      required(path.longitude, { message: 'Pick the location on the map.' });
    },
  );

  // The picked point, or null while none is picked.
  protected readonly location = computed<MapPin | null>(() => {
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    return latitude === null || longitude === null ? null : { latitude, longitude };
  });

  // The map shows the picked point as its only pin.
  protected readonly pins = computed<MapPin[]>(() => {
    const location = this.location();
    return location === null ? [] : [location];
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected pickLocation(pin: MapPin): void {
    this.form.latitude().value.set(pin.latitude);
    this.form.longitude().value.set(pin.longitude);
  }

  protected coordinates(pin: MapPin): string {
    return `${pin.latitude.toFixed(5)}, ${pin.longitude.toFixed(5)}`;
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      // The server expects "HH:mm:ss"; the time input gives "HH:mm".
      // The submit button is enabled only when the form is valid, so the numbers are set.
      await this.socialChallengeManagement.create({
        name: this.form.name().value(),
        activeFrom: `${this.form.activeFrom().value()}:00`,
        activeUntil: `${this.form.activeUntil().value()}:00`,
        requiredParticipants: this.form.requiredParticipants().value() ?? 0,
        radiusMeters: this.form.radiusMeters().value() ?? 0,
        experiencePoints: this.form.experiencePoints().value() ?? 0,
        completionMessage: this.form.completionMessage().value(),
        latitude: this.form.latitude().value(),
        longitude: this.form.longitude().value(),
      });
      await this.router.navigate(['/games/social-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
