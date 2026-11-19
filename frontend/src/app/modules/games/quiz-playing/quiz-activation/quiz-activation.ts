import { Component, computed, input, output, signal } from '@angular/core';
import { Map, MapPin } from '../../../../shared/ui/map/map';
import { QuizPlayStateDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-activation',
  imports: [Map],
  templateUrl: './quiz-activation.html',
  styleUrl: './quiz-activation.scss',
})
export class QuizActivation {
  readonly state = input.required<QuizPlayStateDto>();
  readonly pending = input(false);
  // The server's message when activation was refused (too far away, invalid position).
  readonly error = input<string | null>(null);
  readonly activated = output<MapPin>();

  protected readonly position = signal<MapPin | null>(null);
  protected readonly quizPin = computed<MapPin[]>(() => {
    const { latitude, longitude } = this.state();
    return latitude !== null && longitude !== null ? [{ latitude, longitude }] : [];
  });

  protected activate(): void {
    if (this.quizPin().length === 0) {
      this.activated.emit({ latitude: 0, longitude: 0 });
      return;
    }
    const position = this.position();
    if (position) {
      this.activated.emit(position);
    }
  }
}
