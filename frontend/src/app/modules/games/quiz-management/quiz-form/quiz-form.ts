import { Component, computed, input, output, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Map, type MapPin } from '../../../../shared/ui/map/map';
import { SaveQuizDto } from '../../api/games-api-types';

interface QuizQuestionForm {
  id: string | null;
  text: string;
  answers: { text: string }[];
  correctAnswer: string;
}

export function createQuizForm() {
  return form(
    signal({
      name: '',
      instructions: '',
      experiencePoints: null as number | null,
      questions: [] as QuizQuestionForm[],
      location: null as MapPin | null,
    }),
    (path) => {
      required(path.name, { message: 'Name is required.' });
      required(path.instructions, { message: 'Instructions are required.' });
      required(path.experiencePoints, { message: 'Enter the XP.' });
      min(path.experiencePoints, 0, { message: 'XP must be zero or greater.' });
      validate(path.name, ({ value }) =>
        value() && !value().trim()
          ? { kind: 'blank', message: 'Name cannot contain only spaces.' }
          : null,
      );
      validate(path.instructions, ({ value }) =>
        value() && !value().trim()
          ? { kind: 'blank', message: 'Instructions cannot contain only spaces.' }
          : null,
      );
      validate(path.experiencePoints, ({ value }) =>
        value() !== null && !Number.isInteger(value())
          ? { kind: 'integer', message: 'XP must be a whole number.' }
          : null,
      );
    },
  );
}

@Component({
  selector: 'app-quiz-form',
  imports: [FormField, Map, RouterLink],
  templateUrl: './quiz-form.html',
  styleUrl: './quiz-form.scss',
})
export class QuizForm {
  readonly fields = input.required<ReturnType<typeof createQuizForm>>();
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly saveLabel = input.required<string>();
  // The text on the left of the form's footer, e.g. "New quizzes start as drafts."
  readonly note = input.required<string>();
  // Where Cancel leads; without it, Cancel raises the cancelled output instead.
  readonly cancelLink = input<string | null>(null);
  readonly save = output<SaveQuizDto>();
  readonly cancelled = output<void>();

  // The map shows the picked location as its only pin.
  protected readonly locationPins = computed<MapPin[]>(() => {
    const location = this.fields().location().value();
    return location === null ? [] : [location];
  });

  protected addQuestion(): void {
    this.fields()
      .questions()
      .value.update((questions) => [
        ...questions,
        { id: null, text: '', answers: [], correctAnswer: '' },
      ]);
  }

  protected removeQuestion(index: number): void {
    this.fields()
      .questions()
      .value.update((questions) => questions.filter((_, position) => position !== index));
  }

  protected addAnswer(index: number): void {
    this.fields()
      .questions[index]!.answers()
      .value.update((answers) => [...answers, { text: '' }]);
  }

  protected removeAnswer(questionIndex: number, answerIndex: number): void {
    this.fields().questions[questionIndex]!().value.update((question) => {
      const selected = question.correctAnswer === '' ? -1 : Number(question.correctAnswer);
      const correctAnswer =
        selected < 0 || selected === answerIndex
          ? ''
          : String(selected > answerIndex ? selected - 1 : selected);
      return {
        ...question,
        correctAnswer,
        answers: question.answers.filter((_, index) => index !== answerIndex),
      };
    });
  }

  protected submit(event: Event): void {
    event.preventDefault();
    const state = this.fields()();
    if (this.pending() || !state.valid()) {
      return;
    }

    const { name, instructions, experiencePoints, questions, location } = state.value();
    this.save.emit({
      name,
      instructions,
      experiencePoints,
      latitude: location?.latitude ?? null,
      longitude: location?.longitude ?? null,
      questions: questions.map((question) => ({
        id: question.id,
        text: question.text,
        answers: question.answers.map((answer, index) => ({
          text: answer.text,
          isCorrect: question.correctAnswer === String(index),
        })),
      })),
    });
  }
}
