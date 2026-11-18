import { Component, input, output } from '@angular/core';
import { QuizPlayQuestionDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-question',
  templateUrl: './quiz-question.html',
  styleUrl: './quiz-question.scss',
})
export class QuizQuestion {
  readonly question = input.required<QuizPlayQuestionDto>();
  readonly questionNumber = input.required<number>();
  readonly questionCount = input.required<number>();
  readonly points = input.required<number>();
  readonly pending = input(false);
  readonly answered = output<number>();
}
