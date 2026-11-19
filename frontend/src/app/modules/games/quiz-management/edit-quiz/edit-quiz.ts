import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Map, type MapPin } from '../../../../shared/ui/map/map';
import { formatDate } from '../../../../shared/util/format-date';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizDto, QuizQuestionDto, SaveQuizDto } from '../../api/games-api-types';
import { QuizAction, QuizActions } from '../quiz-actions/quiz-actions';
import { QuizForm, createQuizForm } from '../quiz-form/quiz-form';
import { QuizManagement } from '../quiz-management';

// The success message after each lifecycle change.
const doneMessages: Record<QuizAction, string> = {
  publish: 'Quiz published.',
  retire: 'Quiz retired.',
  archive: 'Quiz archived.',
  delete: 'Quiz deleted.',
};

@Component({
  selector: 'app-edit-quiz',
  imports: [Map, QuizActions, QuizForm, RouterLink],
  templateUrl: './edit-quiz.html',
  styleUrl: './edit-quiz.scss',
})
export class EditQuiz {
  private readonly management = inject(QuizManagement);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly formatDate = formatDate;
  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);
  protected readonly locationPins = computed<MapPin[]>(() => {
    if (!this.quiz.hasValue()) {
      return [];
    }
    const { latitude, longitude } = this.quiz.value();
    return latitude === null || longitude === null ? [] : [{ latitude, longitude }];
  });
  protected readonly fields = createQuizForm();
  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  // True while "Are you sure you want to delete the quiz?" replaces the header buttons.
  protected readonly confirmingDelete = signal(false);
  protected readonly success = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly missing = computed(() => {
    const failure = this.quiz.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  protected hasCorrectAnswer(question: QuizQuestionDto): boolean {
    return question.answers.some((answer) => answer.isCorrect);
  }

  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.success.set(null);
    this.fields().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      experiencePoints: quiz.experiencePoints,
      location: this.locationPins()[0] ?? null,
      questions: quiz.questions.map((question) => {
        const correctAnswer = question.answers.findIndex((answer) => answer.isCorrect);
        return {
          id: question.id,
          text: question.text,
          answers: question.answers.map((answer) => ({ text: answer.text })),
          correctAnswer: correctAnswer === -1 ? '' : String(correctAnswer),
        };
      }),
    });
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected async update(dto: SaveQuizDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.management.update(this.id(), dto);
      this.editing.set(false);
      this.success.set('Quiz saved.');
      this.quiz.reload();
    } catch (failure) {
      // The form stays open with what was typed; the message shows at its top.
      this.error.set(serverMessage(failure, 'Could not save the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async changeLifecycle(action: QuizAction): Promise<void> {
    if (this.pending()) {
      return;
    }

    this.pending.set(true);
    this.success.set(null);
    this.error.set(null);
    try {
      await this.management.changeLifecycle(this.id(), action);
      if (action === 'delete') {
        await this.router.navigate(['/games/quizzes']);
        return;
      }
      this.success.set(doneMessages[action]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the quiz.'));
    } finally {
      this.confirmingDelete.set(false);
      this.pending.set(false);
    }
    this.quiz.reload();
  }
}
