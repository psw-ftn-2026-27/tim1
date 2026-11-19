import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { AvailableQuizDto } from '../../api/games-api-types';

@Component({
  selector: 'app-available-quizzes',
  imports: [RouterLink],
  templateUrl: './available-quizzes.html',
  styleUrl: './available-quizzes.scss',
})
export class AvailableQuizzes {
  // Six cards fill two rows of three on a wide screen.
  protected readonly pageSize = 6;
  protected readonly page = signal(1);
  protected readonly quizzes = httpResource<PageResult<AvailableQuizDto>>(
    () => `/api/games/play/quizzes?page=${this.page()}&pageSize=${this.pageSize}`,
  );

  protected totalPages(totalCount: number): number {
    return Math.max(1, Math.ceil(totalCount / this.pageSize));
  }

  // The numbers on the page buttons: 1, 2, ... totalPages.
  protected pageNumbers(totalCount: number): number[] {
    const numbers: number[] = [];
    for (let number = 1; number <= this.totalPages(totalCount); number++) {
      numbers.push(number);
    }
    return numbers;
  }
}
