import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly difficulties: TourDifficulty[] = ['Easy', 'Moderate', 'Hard'];

  protected readonly form = form(
    signal({
      name: '',
      description: '',
      difficulty: 'Easy' as TourDifficulty,
      tagInput: '',
      groupSizeMin: 1,
      groupSizeMax: 10,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.description, { message: 'Description is required.' });
      required(path.groupSizeMin, { message: 'Minimum group size is required.' });
      min(path.groupSizeMin, 1, { message: 'Minimum group size must be at least 1.' });
      required(path.groupSizeMax, { message: 'Maximum group size is required.' });
      min(path.groupSizeMax, ({ valueOf }) => valueOf(path.groupSizeMin), {
        message: 'Maximum group size must not be less than the minimum.',
      });
    },
  );

  // The tags already added; the text still being typed lives in the form.
  protected readonly tags = signal<string[]>([]);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // Enter adds the typed tag. The input is inside the form, so Enter must not submit it.
  protected addTag(event: Event): void {
    event.preventDefault();
    const tag = this.form.tagInput().value().trim();
    this.form.tagInput().value.set('');
    if (tag !== '' && !this.tags().includes(tag)) {
      this.tags.update((tags) => [...tags, tag]);
    }
  }

  protected removeTag(tag: string): void {
    this.tags.update((tags) => tags.filter((t) => t !== tag));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.tags(),
        groupSize: {
          min: this.form.groupSizeMin().value(),
          max: this.form.groupSizeMax().value(),
        },
      });
      await this.router.navigate(['/exploration/mine']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the tour.'));
    } finally {
      this.pending.set(false);
    }
  }
}
