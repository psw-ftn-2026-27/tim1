import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async removeItem(item: CartItemDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.shoppingCart.removeItem({ type: item.type, entityId: item.entityId });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not remove the item from your cart.'));
    } finally {
      this.cart.reload();
      this.pending.set(false);
    }
  }
}
