import { Component, computed, input } from '@angular/core';
import { formatLength } from '../../../../shared/util/format-length';
import { TourDto } from '../../api/exploration-api-types';

// The card shows at most this many tags; the rest are counted as "+N more".
const SHOWN_TAGS = 3;

@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();

  protected readonly formatLength = formatLength;

  // The server calls the middle difficulty "Moderate"; the screen shows "Medium".
  protected readonly difficulty = computed(() =>
    this.tour().difficulty === 'Moderate' ? 'Medium' : this.tour().difficulty,
  );

  protected readonly shownTags = computed(() => this.tour().tags.slice(0, SHOWN_TAGS));
  protected readonly hiddenTagCount = computed(() =>
    Math.max(0, this.tour().tags.length - SHOWN_TAGS),
  );
}
