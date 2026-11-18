import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ActivateQuizDto, AnswerQuestionDto } from '../api/games-api-types';

const BASE_URL = '/api/games/play/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizPlaying {
  private readonly http = inject(HttpClient);

  async activate(quizId: string, dto: ActivateQuizDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${quizId}/activate`, dto));
  }

  async answer(quizId: string, dto: AnswerQuestionDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${quizId}/answers`, dto));
  }
}
