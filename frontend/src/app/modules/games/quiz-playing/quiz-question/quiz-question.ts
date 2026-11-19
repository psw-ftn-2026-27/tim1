import { Component, input, output } from '@angular/core';
import { QuizPlayQuestionDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-question',
  templateUrl: './quiz-question.html',
  styleUrl: './quiz-question.scss',
})
export class QuizQuestion {
  readonly question = input.required<QuizPlayQuestionDto>();
  readonly pending = input(false);
  // The server's message when the answer was refused.
  readonly error = input<string | null>(null);
  readonly answered = output<number>();

  // The letter in front of each answer: A, B, C, D…
  protected readonly letters = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];
}
