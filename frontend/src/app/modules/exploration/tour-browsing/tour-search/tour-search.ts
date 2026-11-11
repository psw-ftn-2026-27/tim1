import { Component, computed, output, signal } from '@angular/core';
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
  readonly criteriaChange = output<TourCriteria>();

  // What the user is typing or has ticked. The title is only sent once Enter is pressed.
  private readonly model = signal({
    title: '',
    tagInput: '',
    easy: false,
    moderate: false,
    hard: false,
    walking: false,
    bicycle: false,
    car: false,
    sort: 'Newest' as TourSortOrder,
  });

  protected readonly form = form(this.model);

  // The title and tags that have been applied (sent to the page).
  private readonly appliedTitle = signal('');
  protected readonly tags = signal<string[]>([]);
  protected readonly filtersOpen = signal(false);

  protected readonly selectedDifficultyCount = computed(() => {
    const m = this.model();
    return [m.easy, m.moderate, m.hard].filter((selected) => selected).length;
  });

  protected toggleFilters(): void {
    this.filtersOpen.update((open) => !open);
  }

  protected applyTitle(): void {
    this.appliedTitle.set(this.model().title.trim());
    this.emitCriteria();
  }

  protected addTag(): void {
    const tag = this.model().tagInput.trim();
    const added = tag !== '' && !this.tags().includes(tag);
    if (added) {
      this.tags.update((tags) => [...tags, tag]);
    }
    this.model.update((m) => ({ ...m, tagInput: '' }));
    if (added) {
      this.emitCriteria();
    }
  }

  protected removeTag(tag: string): void {
    this.tags.update((tags) => tags.filter((t) => t !== tag));
    this.emitCriteria();
  }

  protected difficultyChanged(): void {
    this.model.update((m) => ({ ...m, sort: 'Newest' }));
    this.emitCriteria();
  }

  protected emitCriteria(): void {
    const m = this.model();
    const difficulties: TourDifficulty[] = [];
    if (m.easy) difficulties.push('Easy');
    if (m.moderate) difficulties.push('Moderate');
    if (m.hard) difficulties.push('Hard');

    const transports: TransportMode[] = [];
    if (m.walking) transports.push('Walking');
    if (m.bicycle) transports.push('Bicycle');
    if (m.car) transports.push('Car');

    this.criteriaChange.emit({
      title: this.appliedTitle(),
      difficulties,
      transports,
      tags: this.tags(),
      sort: m.sort,
    });
  }
}
