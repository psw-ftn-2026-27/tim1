import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { CouponDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink, RouterLinkActive],
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

  // Kupon čiji je kod upravo kopiran, da dugme kratko pokaže „Copied"
  protected readonly copiedId = signal<string | null>(null);

  protected pageCount(totalCount: number): number {
    return Math.max(1, Math.ceil(totalCount / this.pageSize));
  }

  // Kopira kod u clipboard, samo u browseru; ne šalje ništa serveru
  protected async copyCode(coupon: CouponDto): Promise<void> {
    await navigator.clipboard.writeText(coupon.code);
    this.copiedId.set(coupon.id);
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('sr-Latn-RS', { timeZone: 'UTC' });
  }
}
