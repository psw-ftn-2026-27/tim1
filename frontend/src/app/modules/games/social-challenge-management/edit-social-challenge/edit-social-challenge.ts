import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Map } from '../../../../shared/ui/map/map';
import type { MapPin } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeManagement } from '../social-challenge-management';

@Component({
  imports: [FormField, Map, RouterLink],
  selector: 'app-edit-social-challenge',
  styleUrl: './edit-social-challenge.scss',
  templateUrl: './edit-social-challenge.html',
})
export class EditSocialChallenge {
  private readonly socialChallengeManagement = inject(SocialChallengeManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly challenge = httpResource<SocialChallengeDto>(
    () => `/api/games/social-challenges/${this.id()}`,
  );

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

  // The read-only map shows the loaded challenge's location as its only pin.
  protected readonly challengePins = computed<MapPin[]>(() =>
    this.challenge.hasValue()
      ? [{ latitude: this.challenge.value().latitude, longitude: this.challenge.value().longitude }]
      : [],
  );

  // While editing: the picked point, or null while none is picked.
  protected readonly location = computed<MapPin | null>(() => {
    const latitude = this.form.latitude().value();
    const longitude = this.form.longitude().value();
    return latitude === null || longitude === null ? null : { latitude, longitude };
  });

  // While editing, the map shows the picked point as its only pin.
  protected readonly pins = computed<MapPin[]>(() => {
    const location = this.location();
    return location === null ? [] : [location];
  });

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // The server sends "HH:mm:ss"; the page shows and edits only hours and minutes.
  protected hoursAndMinutes(time: string): string {
    return time.slice(0, 5);
  }

  protected startEdit(challenge: SocialChallengeDto): void {
    this.error.set(null);
    this.form().reset({
      name: challenge.name,
      activeFrom: this.hoursAndMinutes(challenge.activeFrom),
      activeUntil: this.hoursAndMinutes(challenge.activeUntil),
      requiredParticipants: challenge.requiredParticipants,
      radiusMeters: challenge.radiusMeters,
      experiencePoints: challenge.experiencePoints,
      completionMessage: challenge.completionMessage,
      latitude: challenge.latitude,
      longitude: challenge.longitude,
    });
    this.editing.set(true);
  }

  protected pickLocation(pin: MapPin): void {
    this.form.latitude().value.set(pin.latitude);
    this.form.longitude().value.set(pin.longitude);
  }

  protected coordinates(pin: MapPin): string {
    return `${pin.latitude.toFixed(5)}, ${pin.longitude.toFixed(5)}`;
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      // The server expects "HH:mm:ss"; the time input gives "HH:mm".
      // The submit button is enabled only when the form is valid, so the numbers are set.
      await this.socialChallengeManagement.update(this.id(), {
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
      this.error.set(serverMessage(failure, 'Could not save the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
