import { Routes } from '@angular/router';
import { Wallet } from './wallet-funding/wallet/wallet';
import { WalletRequests } from './wallet-request-review/wallet-requests/wallet-requests';

export const paymentRoutes: Routes = [
  { path: '', component: Wallet },
  { path: 'admin/requests', component: WalletRequests },
];
