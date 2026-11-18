import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { CouponDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-coupon-list',
  styleUrl: './coupon-list.scss',
  templateUrl: './coupon-list.html',
})
export class CouponList {
  protected readonly pageSize = 10;
  protected readonly page = signal(1);

  protected readonly coupons = httpResource<PageResult<CouponDto>>(
    () => `/api/payment/coupons?page=${this.page()}&pageSize=${this.pageSize}`,
  );

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS', { timeZone: 'UTC' });
  }
}
