import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ClubDto } from '../../api/social-api-types';
import { ClubManagement } from '../club-management';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-clubs',
  styleUrl: './my-clubs.scss',
  templateUrl: './my-clubs.html',
})
export class MyClubs {
  private readonly clubManagement = inject(ClubManagement);

  protected readonly clubs = httpResource<ClubDto[]>(() => '/api/social/clubs/mine');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly editingClubId = signal<string | null>(null);
  protected readonly editError = signal<string | null>(null);
  protected readonly deletingClubId = signal<string | null>(null);

  protected readonly editForm = form(
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

  protected startEdit(club: ClubDto): void {
    this.deletingClubId.set(null);
    this.error.set(null);
    this.editError.set(null);
    this.editingClubId.set(club.id);
    this.editForm().reset({
      name: club.name,
      description: club.description,
      images: [...club.images],
    });
  }

  protected cancelEdit(): void {
    this.editingClubId.set(null);
  }

  protected addImage(): void {
    this.editForm.images().value.update((images) => [...images, '']);
  }

  protected removeImage(index: number): void {
    this.editForm.images().value.update((images) => images.filter((_, i) => i !== index));
  }

  protected async save(event: Event, clubId: string): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.editError.set(null);
    try {
      await this.clubManagement.update(clubId, {
        name: this.editForm.name().value(),
        description: this.editForm.description().value(),
        images: this.editForm
          .images()
          .value()
          .map((image) => image.trim())
          .filter((image) => image !== ''),
      });
      this.editingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.editError.set(serverMessage(failure, 'Could not update the club.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected startDelete(clubId: string): void {
    this.editingClubId.set(null);
    this.error.set(null);
    this.deletingClubId.set(clubId);
  }

  protected cancelDelete(): void {
    this.deletingClubId.set(null);
  }

  protected async confirmDelete(clubId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.clubManagement.delete(clubId);
      this.deletingClubId.set(null);
      this.clubs.reload();
    } catch (failure) {
      this.deletingClubId.set(null);
      this.error.set(serverMessage(failure, 'Could not delete the club.'));
    } finally {
      this.pending.set(false);
    }
  }
}
