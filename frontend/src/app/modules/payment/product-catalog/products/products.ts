import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ProductDto } from '../../api/payment-api-types';
import { ProductCatalog } from '../product-catalog';

@Component({
  imports: [RouterLink],
  selector: 'app-products',
  styleUrl: './products.scss',
  templateUrl: './products.html',
})
export class Products {
  private readonly productCatalog = inject(ProductCatalog);

  protected readonly products = httpResource<ProductDto[]>(() => '/api/payment/products');
  protected readonly itemCount = httpResource<number>(() => '/api/payment/cart/item-count');

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly added = signal<string | null>(null);

  protected async addToCart(product: ProductDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.added.set(null);
    try {
      await this.productCatalog.addToCart({ type: product.type, entityId: product.entityId });
      this.added.set(`${product.name} is in your cart.`);
      this.products.reload();
      this.itemCount.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the product to your cart.'));
    } finally {
      this.pending.set(false);
    }
  }
}
