import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
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

  // An empty productId means "any product" (any product of the owner).
  protected readonly form = form(
    signal({
      productId: '',
      percent: 10,
      expiresOn: '',
    }),
    (path) => {
      required(path.percent, { message: 'The discount percent is required.' });
      min(path.percent, 1, { message: 'The discount percent must be between 1 and 100.' });
      max(path.percent, 100, { message: 'The discount percent must be between 1 and 100.' });
      required(path.expiresOn, { message: 'The expiry date is required.' });
    },
  );

  protected readonly today = new Date().toISOString().slice(0, 10);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.coupons.createCoupon({
        productId: this.form.productId().value() || null,
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
