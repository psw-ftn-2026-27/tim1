import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizDto, SaveQuizDto } from '../../api/games-api-types';
import { QuizForm, createQuizForm } from '../quiz-form/quiz-form';
import { QuizManagement } from '../quiz-management';

@Component({
  selector: 'app-edit-quiz',
  imports: [QuizForm, RouterLink],
  templateUrl: './edit-quiz.html',
  styleUrl: './edit-quiz.scss',
})
export class EditQuiz {
  private readonly management = inject(QuizManagement);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);
  protected readonly fields = createQuizForm();
  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly saved = signal(false);
  protected readonly error = signal<string | null>(null);

  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.saved.set(false);
    this.fields().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      experiencePoints: quiz.experiencePoints,
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

  protected async update(dto: SaveQuizDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.management.update(this.id(), dto);
      this.editing.set(false);
      this.saved.set(true);
      this.quiz.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the quiz.'));
    } finally {
      this.pending.set(false);
    }
  }
}
