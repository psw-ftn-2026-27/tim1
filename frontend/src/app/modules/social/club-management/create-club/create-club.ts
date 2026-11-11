import { Component, inject, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubManagement } from '../club-management';

@Component({
  imports: [FormField],
  selector: 'app-create-club',
  styleUrl: './create-club.scss',
  templateUrl: './create-club.html',
})
export class CreateClub {
  private readonly clubManagement = inject(ClubManagement);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({ name: '', description: '', images: [''] }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      validate(path.images, ({ value }) =>
        value().some((image) => image.trim() !== '')
          ? null
          : { kind: 'noImage', message: 'Add at least one image address.' },
      );
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected addImage(): void {
    this.form.images().value.update((images) => [...images, '']);
  }

  protected removeImage(index: number): void {
    this.form.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        images: this.form
          .images()
          .value()
          .map((image) => image.trim())
          .filter((image) => image !== ''),
      });
      await this.router.navigate(['/social/clubs/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
