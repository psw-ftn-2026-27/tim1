import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { GiftCardDto, GiftCardStatus } from '../../api/payment-api-types';

const STATUS_LABELS: Record<GiftCardStatus, string> = {
  Valid: 'valid',
  Used: 'used',
  Expired: 'expired',
};

@Component({
  imports: [RouterLink],
  selector: 'app-gift-card-list',
  styleUrl: './gift-card-list.scss',
  templateUrl: './gift-card-list.html',
})
export class GiftCardList {
  protected readonly pageSize = 10;
  protected readonly createdPage = signal(1);
  protected readonly receivedPage = signal(1);

  protected readonly created = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/created?page=${this.createdPage()}&pageSize=${this.pageSize}`,
  );
  protected readonly received = httpResource<PageResult<GiftCardDto>>(
    () => `/api/payment/gift-cards/received?page=${this.receivedPage()}&pageSize=${this.pageSize}`,
  );

  protected statusLabel(status: GiftCardStatus): string {
    return STATUS_LABELS[status];
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS');
  }
}
