import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-tour-pagination',
  styleUrl: './tour-pagination.scss',
  templateUrl: './tour-pagination.html',
})
export class TourPagination {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();

  readonly pageChange = output<number>();

  protected readonly totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));

  protected goTo(target: number): void {
    if (target < 1 || target > this.totalPages() || target === this.page()) {
      return;
    }
    this.pageChange.emit(target);
  }
}
