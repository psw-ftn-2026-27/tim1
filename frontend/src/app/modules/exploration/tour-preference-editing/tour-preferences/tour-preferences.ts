import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  TourDifficulty,
  TourPreferenceDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourPreferenceEditing } from '../tour-preference-editing';

@Component({
  imports: [FormField],
  selector: 'app-tour-preferences',
  styleUrl: './tour-preferences.scss',
  templateUrl: './tour-preferences.html',
})
export class TourPreferences {
  private readonly tourPreferenceEditing = inject(TourPreferenceEditing);

  protected readonly preferences = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/tour-preferences/mine',
  );

  protected readonly availableTags = httpResource<string[]>(
    () => '/api/exploration/tour-preferences/available-tags',
  );

  protected readonly editing = signal(false);
  protected readonly saved = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({
    easy: false,
    moderate: false,
    hard: false,
    transport: '',
    tags: [] as string[],
    opensSearch: false,
  });

  protected readonly form = form(this.model);

  protected readonly offeredTags = computed<string[]>(() => {
    const offered = [...(this.availableTags.value() ?? [])];
    for (const tag of this.preferences.value()?.tags ?? []) {
      if (!this.contains(offered, tag)) {
        offered.push(tag);
      }
    }
    return offered;
  });

  protected readonly isEmpty = computed(() => {
    const model = this.model();
    return (
      !model.easy &&
      !model.moderate &&
      !model.hard &&
      model.transport === '' &&
      model.tags.length === 0
    );
  });

  protected startEdit(): void {
    const current = this.preferences.value();
    this.saved.set(false);
    this.error.set(null);
    this.editing.set(true);
    this.form().reset({
      easy: current?.difficulties.includes('Easy') ?? false,
      moderate: current?.difficulties.includes('Moderate') ?? false,
      hard: current?.difficulties.includes('Hard') ?? false,
      transport: current?.transport ?? '',
      tags: current?.tags ?? [],
      opensSearch: current?.opensSearch ?? false,
    });
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected isSelected(tag: string): boolean {
    return this.contains(this.model().tags, tag);
  }

  protected toggleTag(tag: string): void {
    this.model.update((model) => ({
      ...model,
      tags: this.contains(model.tags, tag)
        ? model.tags.filter((selected) => selected.toLowerCase() !== tag.toLowerCase())
        : [...model.tags, tag],
    }));
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const model = this.model();
    const difficulties: TourDifficulty[] = [];
    if (model.easy) {
      difficulties.push('Easy');
    }
    if (model.moderate) {
      difficulties.push('Moderate');
    }
    if (model.hard) {
      difficulties.push('Hard');
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourPreferenceEditing.save({
        difficulties,
        transport: model.transport === '' ? null : (model.transport as TransportMode),
        tags: model.tags,
        opensSearch: model.opensSearch,
      });
      this.preferences.reload();
      this.editing.set(false);
      this.saved.set(true);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async delete(): Promise<void> {
    this.pending.set(true);
    this.saved.set(false);
    this.error.set(null);
    try {
      await this.tourPreferenceEditing.delete();
      this.preferences.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  private contains(tags: string[], tag: string): boolean {
    return tags.some((existing) => existing.toLowerCase() === tag.toLowerCase());
  }
}
