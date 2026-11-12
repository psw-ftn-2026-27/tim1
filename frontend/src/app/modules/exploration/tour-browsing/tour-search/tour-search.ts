import { Component, computed, input, output, signal } from '@angular/core';
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
  private readonly model = signal({ title: '', tagInput: '' });

  protected readonly form = form(this.model);

  // Until the user opens or closes the filters, they are open when the criteria use them.
  private readonly filtersToggled = signal<boolean | null>(null);
  protected readonly filtersOpen = computed(() => {
    const c = this.criteria();
    return this.filtersToggled() ?? (c.difficulties.length > 0 || c.transports.length > 0);
  });

  protected toggleFilters(): void {
    this.filtersToggled.set(!this.filtersOpen());
  }

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

  protected changeSort(event: Event): void {
    const sort = (event.target as HTMLSelectElement).value as TourSortOrder;
    this.criteriaChange.emit({ ...this.criteria(), sort });
  }

  private toggled<T>(values: T[], value: T): T[] {
    return values.includes(value) ? values.filter((v) => v !== value) : [...values, value];
  }
}
