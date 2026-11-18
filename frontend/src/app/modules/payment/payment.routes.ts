import { Routes } from '@angular/router';
import { CouponList } from './coupons/coupon-list/coupon-list';
import { CreateCoupon } from './coupons/create-coupon/create-coupon';
import { BuyGiftCard } from './gift-cards/buy-gift-card/buy-gift-card';
import { ChooseRecipient } from './gift-cards/choose-recipient/choose-recipient';
import { GiftCardList } from './gift-cards/gift-card-list/gift-card-list';
import { Products } from './product-catalog/products/products';
import { Cart } from './shopping-cart/cart/cart';
import { Wallet } from './wallet-funding/wallet/wallet';
import { WalletRequests } from './wallet-request-review/wallet-requests/wallet-requests';

export const paymentRoutes: Routes = [
  { path: '', component: Wallet },
  { path: 'gift-cards', component: GiftCardList },
  { path: 'gift-cards/buy', component: ChooseRecipient },
  { path: 'gift-cards/buy/:recipientId', component: BuyGiftCard },
  { path: 'products', component: Products },
  { path: 'coupons', component: CouponList },
  { path: 'coupons/create', component: CreateCoupon },
  { path: 'cart', component: Cart },
  { path: 'admin/requests', component: WalletRequests },
];
