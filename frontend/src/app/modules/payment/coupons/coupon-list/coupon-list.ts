import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CouponDto } from '../../api/payment-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-coupon-list',
  styleUrl: './coupon-list.scss',
  templateUrl: './coupon-list.html',
})
export class CouponList {
  protected readonly coupons = httpResource<CouponDto[]>(() => '/api/payment/coupons');
}
