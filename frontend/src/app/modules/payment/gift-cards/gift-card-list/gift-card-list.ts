import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { formatAmount } from '../../../../shared/util/format-amount';
import { GiftCardDto, GiftCardStatus } from '../../api/payment-api-types';
import { GiftCardArt } from '../gift-card-art/gift-card-art';

const STATUS_LABELS: Record<GiftCardStatus, string> = {
  Valid: 'Valid',
  Used: 'Used',
  Expired: 'Expired',
};

const STATUS_BADGES: Record<GiftCardStatus, string> = {
  Valid: 'badge--success',
  Used: '',
  Expired: 'badge--danger',
};

@Component({
  imports: [GiftCardArt, RouterLink, RouterLinkActive],
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

  protected readonly formatAmount = formatAmount;

  protected statusBadge(status: GiftCardStatus): string {
    return STATUS_BADGES[status];
  }

  protected statusLabel(status: GiftCardStatus): string {
    return STATUS_LABELS[status];
  }

  // Datum na kartici zavisi od statusa: važeća ističe, iskorišćena je važila, istekla je istekla
  protected dateLabel(card: GiftCardDto): string {
    const date = this.formatDate(card.expiresAt);
    if (card.status === 'Used') {
      return `Used · valid until ${date}`;
    }
    return card.status === 'Expired' ? `Expired ${date}` : `Expires ${date}`;
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS', { timeZone: 'UTC' });
  }
}
