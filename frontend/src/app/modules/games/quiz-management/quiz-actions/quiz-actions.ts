import { Component, input, output, signal } from '@angular/core';
import { QuizStatus } from '../../api/games-api-types';

export type QuizAction = 'publish' | 'retire' | 'archive' | 'delete';

@Component({
  selector: 'app-quiz-actions',
  templateUrl: './quiz-actions.html',
  styleUrl: './quiz-actions.scss',
})
export class QuizActions {
  readonly status = input.required<QuizStatus>();
  readonly pending = input(false);
  readonly requested = output<QuizAction>();
  protected readonly confirmingDelete = signal(false);

  protected confirmDelete(): void {
    this.confirmingDelete.set(false);
    this.requested.emit('delete');
  }
}
