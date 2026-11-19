import { Component, input, output } from '@angular/core';
import { QuizStatus } from '../../api/games-api-types';

export type QuizAction = 'publish' | 'retire' | 'archive' | 'delete';

// The buttons a quiz offers in its current state. The page keeps whether the delete
// confirmation is open, so the table row can also change its background.
@Component({
  selector: 'app-quiz-actions',
  templateUrl: './quiz-actions.html',
  styleUrl: './quiz-actions.scss',
})
export class QuizActions {
  readonly status = input.required<QuizStatus>();
  readonly pending = input(false);
  // True while "Are you sure you want to delete the quiz?" replaces the buttons.
  readonly confirming = input(false);
  // The details page also offers Edit for a draft; the list does not.
  readonly withEdit = input(false);
  // Small buttons in a table row, normal ones in the page header.
  readonly small = input(false);
  readonly requested = output<QuizAction>();
  readonly edit = output<void>();
  readonly confirmingChange = output<boolean>();
}
