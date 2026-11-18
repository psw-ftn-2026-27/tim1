import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto, CartItemDto } from '../../api/payment-api-types';
import { ShoppingCart } from '../shopping-cart';

@Component({
  imports: [RouterLink],
  selector: 'app-cart',
  styleUrl: './cart.scss',
  templateUrl: './cart.html',
})
export class Cart {
  private readonly shoppingCart = inject(ShoppingCart);

  protected readonly cart = httpResource<CartDto>(() => '/api/payment/cart');

  protected readonly hasItems = computed(() => (this.cart.value()?.items.length ?? 0) > 0);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly bought = signal<string | null>(null);
  protected readonly buyError = signal<string | null>(null);

  protected async buyProducts(): Promise<void> {
    this.pending.set(true);
    this.clearMessages();
    try {
      await this.shoppingCart.checkout();
      this.bought.set('Your products are paid for from your wallet.');
      this.cart.reload();
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
      this.pending.set(false);
    }
  }

  private clearMessages(): void {
    this.error.set(null);
    this.bought.set(null);
    this.buyError.set(null);
  }
}
