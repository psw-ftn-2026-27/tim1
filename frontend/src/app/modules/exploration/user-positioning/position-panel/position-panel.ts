import { Component, computed, input, output } from '@angular/core';
import { Map, MapPin } from '../../../../shared/ui/map/map';

@Component({
  selector: 'app-position-panel',
  imports: [Map],
  templateUrl: './position-panel.html',
  styleUrl: './position-panel.scss',
})
export class PositionPanel {
  readonly position = input.required<MapPin | null>();
  readonly busy = input.required<boolean>();
  readonly message = input.required<string>();
  readonly readFailed = input.required<boolean>();
  readonly selected = output<MapPin>();
  readonly retry = output<void>();
  readonly closed = output<void>();

  protected readonly pins = computed(() => {
    const position = this.position();
    return position ? [position] : [];
  });
}
