import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SaveQuizDto } from '../../api/games-api-types';
import { QuizForm, createQuizForm } from '../quiz-form/quiz-form';
import { QuizManagement } from '../quiz-management';

@Component({
  selector: 'app-create-quiz',
  imports: [QuizForm, RouterLink],
  templateUrl: './create-quiz.html',
  styleUrl: './create-quiz.scss',
})
export class CreateQuiz {
  private readonly management = inject(QuizManagement);
  private readonly router = inject(Router);

  protected readonly fields = createQuizForm();
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async create(dto: SaveQuizDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = await this.management.create(dto);
      await this.router.navigate(['/games/quizzes', id, 'edit']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}
