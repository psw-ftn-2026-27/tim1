import { Routes } from '@angular/router';
import { Products } from './product-catalog/products/products';
import { Cart } from './shopping-cart/cart/cart';
import { Wallet } from './wallet-funding/wallet/wallet';
import { WalletRequests } from './wallet-request-review/wallet-requests/wallet-requests';

export const paymentRoutes: Routes = [
  { path: '', component: Wallet },
  { path: 'products', component: Products },
  { path: 'cart', component: Cart },
  { path: 'admin/requests', component: WalletRequests },
];
