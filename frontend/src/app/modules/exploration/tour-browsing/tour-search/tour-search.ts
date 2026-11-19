import { Component, input, linkedSignal, output } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { TourDifficulty, TransportMode } from '../../api/exploration-api-types';

export type TourSortOrder = 'Newest' | 'DifficultyAscending' | 'DifficultyDescending';

export interface TourCriteria {
  title: string;
  difficulties: TourDifficulty[];
  transports: TransportMode[];
  tags: string[];
  sort: TourSortOrder;
}

@Component({
  imports: [FormField],
  selector: 'app-tour-search',
  styleUrl: './tour-search.scss',
  templateUrl: './tour-search.html',
})
export class TourSearch {
  // The applied criteria belong to the page. This component only shows them and reports changes.
  readonly criteria = input.required<TourCriteria>();
  readonly criteriaChange = output<TourCriteria>();

  // Only the text being typed lives here. The title is only sent once Enter is pressed.
  // When the applied criteria change (for example "Reset all"), the field shows the applied title again.
  private readonly model = linkedSignal(() => ({ title: this.criteria().title, tagInput: '' }));

  protected readonly form = form(this.model);

  // The server calls the middle difficulty "Moderate"; the screen shows "Medium".
  protected readonly difficulties: { value: TourDifficulty; label: string }[] = [
    { value: 'Easy', label: 'Easy' },
    { value: 'Moderate', label: 'Medium' },
    { value: 'Hard', label: 'Hard' },
  ];
  protected readonly transports: TransportMode[] = ['Walking', 'Bicycle', 'Car'];

  protected applyTitle(): void {
    this.criteriaChange.emit({ ...this.criteria(), title: this.model().title.trim() });
  }

  protected addTag(): void {
    const tag = this.model().tagInput.trim();
    const tags = this.criteria().tags;
    this.model.update((m) => ({ ...m, tagInput: '' }));
    if (tag !== '' && !tags.includes(tag)) {
      this.criteriaChange.emit({ ...this.criteria(), tags: [...tags, tag] });
    }
  }

  protected removeTag(tag: string): void {
    const tags = this.criteria().tags.filter((t) => t !== tag);
    this.criteriaChange.emit({ ...this.criteria(), tags });
  }

  protected toggleDifficulty(difficulty: TourDifficulty): void {
    const difficulties = this.toggled(this.criteria().difficulties, difficulty);
    this.criteriaChange.emit({ ...this.criteria(), difficulties, sort: 'Newest' });
  }

  protected toggleTransport(transport: TransportMode): void {
    const transports = this.toggled(this.criteria().transports, transport);
    this.criteriaChange.emit({ ...this.criteria(), transports });
  }

  // "All" is not a value, it only removes the filter.
  protected clearDifficulties(): void {
    this.criteriaChange.emit({ ...this.criteria(), difficulties: [] });
  }

  protected clearFilters(): void {
    this.criteriaChange.emit({
      title: '',
      difficulties: [],
      transports: [],
      tags: [],
      sort: 'Newest',
    });
  }

  private toggled<T>(values: T[], value: T): T[] {
    return values.includes(value) ? values.filter((v) => v !== value) : [...values, value];
  }
}
