import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GiftCardStatus, GiftCardsDto } from '../../api/payment-api-types';

const STATUS_LABELS: Record<GiftCardStatus, string> = {
  Valid: 'važeća',
  Used: 'iskorišćena',
  Expired: 'istekla',
};

@Component({
  imports: [RouterLink],
  selector: 'app-gift-card-list',
  styleUrl: './gift-card-list.scss',
  templateUrl: './gift-card-list.html',
})
export class GiftCardList {
  protected readonly giftCards = httpResource<GiftCardsDto>(() => '/api/payment/gift-cards');

  protected statusLabel(status: GiftCardStatus): string {
    return STATUS_LABELS[status];
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS');
  }
}
