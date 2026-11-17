import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { QuizSummaryDto } from '../../api/games-api-types';

@Component({
  selector: 'app-quizzes',
  imports: [RouterLink],
  templateUrl: './quizzes.html',
  styleUrl: './quizzes.scss',
})
export class Quizzes {
  protected readonly pageSize = 20;
  protected readonly page = signal(1);
  protected readonly quizzes = httpResource<PageResult<QuizSummaryDto>>(
    () => `/api/games/quizzes?page=${this.page()}&pageSize=${this.pageSize}`,
  );
}
