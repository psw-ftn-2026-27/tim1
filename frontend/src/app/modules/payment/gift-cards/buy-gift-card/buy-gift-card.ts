import { httpResource } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { RecipientDto } from '../../api/payment-api-types';
import { GiftCards } from '../gift-cards';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-buy-gift-card',
  styleUrl: './buy-gift-card.scss',
  templateUrl: './buy-gift-card.html',
})
export class BuyGiftCard {
  private readonly giftCards = inject(GiftCards);
  private readonly router = inject(Router);

  readonly recipientId = input.required<string>();

  protected readonly recipient = httpResource<RecipientDto>(
    () => `/api/payment/gift-cards/recipients/${this.recipientId()}`,
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly purchaseModel = signal({ amount: 10, message: '' });

  protected readonly purchaseForm = form(this.purchaseModel, (path) => {
    required(path.amount, { message: 'The amount is required.' });
    min(path.amount, 10, { message: 'The amount must be at least 10 EC.' });
    max(path.amount, 1000, { message: 'The amount can be at most 1000 EC.' });
    maxLength(path.message, 200, { message: 'The message can have at most 200 characters.' });
  });

  protected async purchase(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.giftCards.purchase({
        recipientId: this.recipientId(),
        amount: this.purchaseForm.amount().value(),
        message: this.purchaseForm.message().value(),
      });
      await this.router.navigateByUrl('/payment/gift-cards');
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The gift card was not bought.'));
    } finally {
      this.pending.set(false);
    }
  }
}
