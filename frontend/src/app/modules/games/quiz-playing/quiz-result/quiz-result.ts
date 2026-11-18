import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { QuizPlayStateDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quiz-result',
  imports: [RouterLink],
  templateUrl: './quiz-result.html',
  styleUrl: './quiz-result.scss',
})
export class QuizResult {
  readonly state = input.required<QuizPlayStateDto>();
}
