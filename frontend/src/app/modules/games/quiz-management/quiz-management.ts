import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SaveQuizDto } from '../api/games-api-types';
import { QuizAction } from './quiz-actions/quiz-actions';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizManagement {
  private readonly http = inject(HttpClient);

  create(dto: SaveQuizDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async update(id: string, dto: SaveQuizDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async changeLifecycle(id: string, action: QuizAction): Promise<void> {
    const url = `${BASE_URL}/${id}`;
    if (action === 'delete') {
      await firstValueFrom(this.http.delete<void>(url));
    } else {
      await firstValueFrom(this.http.post<void>(`${url}/${action}`, null));
    }
  }
}
