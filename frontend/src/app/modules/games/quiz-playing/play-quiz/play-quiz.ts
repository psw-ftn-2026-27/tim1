import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MapPin } from '../../../../shared/ui/map/map';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizPlayStateDto } from '../../api/games-api-types';
import { QuizActivation } from '../quiz-activation/quiz-activation';
import { QuizPlaying } from '../quiz-playing';
import { QuizQuestion } from '../quiz-question/quiz-question';
import { QuizResult } from '../quiz-result/quiz-result';

@Component({
  selector: 'app-play-quiz',
  imports: [QuizActivation, QuizQuestion, QuizResult, RouterLink],
  templateUrl: './play-quiz.html',
  styleUrl: './play-quiz.scss',
})
export class PlayQuiz {
  private readonly playing = inject(QuizPlaying);

  readonly id = input.required<string>();

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  // Set only after this page's own successful answer, so a reopened page starts at the question.
  protected readonly showFeedback = signal(false);
  protected readonly quiz = httpResource<QuizPlayStateDto>(
    () => `/api/games/play/quizzes/${this.id()}`,
  );

  protected async activate(position: MapPin): Promise<void> {
    if (this.pending()) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    try {
      await this.playing.activate(this.id(), position);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not activate the quiz.'));
    } finally {
      this.quiz.reload();
      this.pending.set(false);
    }
  }

  protected async answer(questionId: string, answerIndex: number): Promise<void> {
    if (this.pending()) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    try {
      await this.playing.answer(this.id(), { questionId, answerIndex });
      this.showFeedback.set(true);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the answer.'));
    } finally {
      this.quiz.reload();
      this.pending.set(false);
    }
  }
}
