import { Component, computed, input, output } from '@angular/core';
import { QuizPlayQuestionDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-question',
  templateUrl: './quiz-question.html',
  styleUrl: './quiz-question.scss',
})
export class QuizQuestion {
  readonly quizName = input.required<string>();
  readonly question = input.required<QuizPlayQuestionDto>();
  readonly questionNumber = input.required<number>();
  readonly questionCount = input.required<number>();
  readonly points = input.required<number>();
  readonly pending = input(false);
  // The server's message when the answer was refused.
  readonly error = input<string | null>(null);
  readonly answered = output<number>();

  // The letter in front of each answer: A, B, C, D…
  protected readonly letters = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];

  // How far through the quiz the player is, for the progress bar.
  protected readonly percentDone = computed(() =>
    this.questionCount() === 0 ? 0 : (this.questionNumber() / this.questionCount()) * 100,
  );
}
