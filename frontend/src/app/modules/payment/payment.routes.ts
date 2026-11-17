import { Routes } from '@angular/router';
import { Products } from './product-catalog/products/products';
import { Wallet } from './wallet-funding/wallet/wallet';
import { WalletRequests } from './wallet-request-review/wallet-requests/wallet-requests';

export const paymentRoutes: Routes = [
  { path: '', component: Wallet },
  { path: 'products', component: Products },
  { path: 'admin/requests', component: WalletRequests },
];
