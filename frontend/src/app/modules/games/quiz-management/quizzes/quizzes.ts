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
  // The quiz whose row shows the delete confirmation.
  protected readonly deletingId = signal<string | null>(null);
  // The server's message for a refused action, shown right under that quiz's row.
  protected readonly rowFailure = signal<{ id: string; message: string } | null>(null);
  protected readonly quizzes = httpResource<PageResult<QuizSummaryDto>>(
    () => `/api/games/quizzes?page=${this.page()}&pageSize=${this.pageSize}`,
  );

  protected goToPage(page: number): void {
    this.deletingId.set(null);
    this.rowFailure.set(null);
    this.page.set(page);
  }

  protected setDeleting(id: string, confirming: boolean): void {
    this.rowFailure.set(null);
    this.deletingId.set(confirming ? id : null);
  }

  protected async changeLifecycle(quiz: QuizSummaryDto, action: QuizAction): Promise<void> {
    if (this.pending()) {
      return;
    }

    const lastQuizOnPage = this.quizzes.value()?.items.length === 1;
    this.pending.set(true);
    this.rowFailure.set(null);
    let removed = false;
    try {
      await this.management.changeLifecycle(quiz.id, action);
      removed = action === 'delete';
    } catch (failure) {
      this.rowFailure.set({
        id: quiz.id,
        message: serverMessage(failure, 'The server refused the change.'),
      });
      removed = failure instanceof HttpErrorResponse && failure.status === 404;
    } finally {
      this.deletingId.set(null);
      // Load the list again, so the actions match each quiz's current state.
      if (removed && lastQuizOnPage && this.page() > 1) {
        this.page.update((page) => page - 1);
      } else {
        this.quizzes.reload();
      }
      this.pending.set(false);
    }
  }
}
