import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { OwnedProductDto } from '../../api/payment-api-types';
import { Coupons } from '../coupons';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-coupon',
  styleUrl: './create-coupon.scss',
  templateUrl: './create-coupon.html',
})
export class CreateCoupon {
  private readonly coupons = inject(Coupons);
  private readonly router = inject(Router);

  protected readonly ownedProducts = httpResource<OwnedProductDto[]>(
    () => '/api/payment/coupons/owned-products',
  );

  // scope bira između jednog proizvoda i bilo kog proizvoda vlasnika. Šalje se samo productId:
  // za „any" je null, kao i ranije kada je izbor bio prazan.
  protected readonly form = form(
    signal({
      scope: 'any',
      productId: '',
      percent: 10,
      expiresOn: '',
    }),
    (path) => {
      required(path.productId, {
        message: 'Choose a product.',
        when: ({ valueOf }) => valueOf(path.scope) === 'one',
      });
      required(path.percent, { message: 'The discount percent is required.' });
      min(path.percent, 1, { message: 'The discount percent must be between 1 and 100.' });
      max(path.percent, 100, { message: 'The discount percent must be between 1 and 100.' });
      required(path.expiresOn, { message: 'The expiry date is required.' });
    },
  );

  protected readonly today = new Date().toISOString().slice(0, 10);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // Naziv izabranog proizvoda za pregled kupona
  protected readonly previewProduct = computed(() => {
    if (this.form.scope().value() === 'any') {
      return 'Any of your products';
    }
    const id = this.form.productId().value();
    return this.ownedProducts.value()?.find((product) => product.id === id)?.name ?? 'Choose a product';
  });

  protected formatDate(isoDate: string): string {
    return new Date(`${isoDate}T00:00:00Z`).toLocaleDateString('sr-Latn-RS', { timeZone: 'UTC' });
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      const oneProduct = this.form.scope().value() === 'one';
      await this.coupons.createCoupon({
        productId: oneProduct ? this.form.productId().value() || null : null,
        percent: this.form.percent().value(),
        // A date input gives YYYY-MM-DD; the coupon is valid until the end of that day (UTC).
        expiresAt: new Date(`${this.form.expiresOn().value()}T23:59:59Z`).toISOString(),
      });
      await this.router.navigate(['/payment/coupons']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'The coupon was not created.'));
    } finally {
      this.pending.set(false);
    }
  }
}
