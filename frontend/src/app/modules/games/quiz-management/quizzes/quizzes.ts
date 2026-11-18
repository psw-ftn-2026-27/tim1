import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizSummaryDto } from '../../api/games-api-types';
import { QuizAction, QuizActions } from '../quiz-actions/quiz-actions';
import { QuizManagement } from '../quiz-management';

@Component({
  selector: 'app-quizzes',
  imports: [QuizActions, RouterLink],
  templateUrl: './quizzes.html',
  styleUrl: './quizzes.scss',
})
export class Quizzes {
  private readonly management = inject(QuizManagement);

  protected readonly pageSize = 20;
  protected readonly page = signal(1);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly confirmation = signal<string | null>(null);
  protected readonly quizzes = httpResource<PageResult<QuizSummaryDto>>(
    () => `/api/games/quizzes?page=${this.page()}&pageSize=${this.pageSize}`,
  );

  protected async changeLifecycle(quiz: QuizSummaryDto, action: QuizAction): Promise<void> {
    if (this.pending()) {
      return;
    }

    const lastQuizOnPage = this.quizzes.value()?.items.length === 1;
    this.pending.set(true);
    this.error.set(null);
    this.confirmation.set(null);
    let removed = false;
    try {
      await this.management.changeLifecycle(quiz.id, action);
      removed = action === 'delete';
      this.confirmation.set(removed ? `${quiz.name} was deleted.` : `Status updated for ${quiz.name}.`);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the quiz.'));
      removed = failure instanceof HttpErrorResponse && failure.status === 404;
    } finally {
      if (removed && lastQuizOnPage && this.page() > 1) {
        this.page.update((page) => page - 1);
      } else {
        this.quizzes.reload();
      }
      this.pending.set(false);
    }
  }
}
