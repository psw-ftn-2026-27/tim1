import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto, CartItemDto, PriceDto, UsableGiftCardDto } from '../../api/payment-api-types';
import { ShoppingCart } from '../shopping-cart';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-cart',
  styleUrl: './cart.scss',
  templateUrl: './cart.html',
})
export class Cart {
  private readonly shoppingCart = inject(ShoppingCart);

  protected readonly cart = httpResource<CartDto>(() => '/api/payment/cart');

  protected readonly hasItems = computed(() => (this.cart.value()?.items.length ?? 0) > 0);

  // The typed code and the chosen card; an empty value means none.
  protected readonly discounts = form(signal({ couponCode: '', giftCardId: '' }));

  // The code counts only after the buyer clicks "Apply coupon".
  protected readonly appliedCode = signal('');

  protected readonly usableGiftCards = httpResource<UsableGiftCardDto[]>(() =>
    this.hasItems() ? '/api/payment/cart/usable-gift-cards' : undefined,
  );

  // The server calculates the price again whenever the applied code or the chosen card changes.
  protected readonly price = httpResource<PriceDto>(() => {
    if (!this.hasItems()) {
      return undefined;
    }
    const params = new URLSearchParams();
    if (this.appliedCode() !== '') {
      params.set('couponCode', this.appliedCode());
    }
    const giftCardId = this.discounts.giftCardId().value();
    if (giftCardId !== '') {
      params.set('giftCardId', giftCardId);
    }
    return `/api/payment/cart/price?${params.toString()}`;
  });

  protected readonly priceError = computed(() =>
    serverMessage(this.price.error(), 'Could not calculate the price. Try again later.'),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly bought = signal<string | null>(null);
  protected readonly buyError = signal<string | null>(null);

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS');
  }

  protected applyCoupon(): void {
    this.appliedCode.set(this.discounts.couponCode().value().trim());
  }

  protected async buyProducts(): Promise<void> {
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.shoppingCart.checkout(
        this.appliedCode() || undefined,
        this.discounts.giftCardId().value() || undefined,
      );
      this.bought.set('Your products are paid for from your wallet.');
      this.discounts().reset({ couponCode: '', giftCardId: '' });
      this.appliedCode.set('');
      this.cart.reload();
      this.price.reload();
      this.usableGiftCards.reload();
    } catch (failure) {
      this.buyError.set(serverMessage(failure, 'Could not buy the products in your cart.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeItem(item: CartItemDto): Promise<void> {
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.shoppingCart.removeItem({ type: item.type, entityId: item.entityId });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the item from your cart.'));
    } finally {
      this.cart.reload();
      this.price.reload();
      this.pending.set(false);
    }
  }

  private clearMessages(): void {
    this.error.set(null);
    this.bought.set(null);
    this.buyError.set(null);
  }
}
