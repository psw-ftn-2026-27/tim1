import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { serverMessage } from '../../../../shared/util/server-message';
import { WalletRequestReviewDto, WalletRequestsDto } from '../../api/payment-api-types';
import { RequestList, RequestRejection } from '../request-list/request-list';
import { WalletRequestReview } from '../wallet-request-review';

@Component({
  imports: [RequestList],
  selector: 'app-wallet-requests',
  styleUrl: './wallet-requests.scss',
  templateUrl: './wallet-requests.html',
})
export class WalletRequests {
  private readonly walletRequestReview = inject(WalletRequestReview);

  protected readonly requests = httpResource<WalletRequestsDto>(
    () => '/api/payment/wallet/requests',
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async approve(request: WalletRequestReviewDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      if (request.type === 'TopUp') {
        await this.walletRequestReview.approveTopUp(request.id);
      } else {
        await this.walletRequestReview.approveWithdrawal(request.id);
      }
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not approve the request.'));
    } finally {
      this.requests.reload();
      this.pending.set(false);
    }
  }

  protected async reject(rejection: RequestRejection): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    const dto = { reason: rejection.reason };
    try {
      if (rejection.request.type === 'TopUp') {
        await this.walletRequestReview.rejectTopUp(rejection.request.id, dto);
      } else {
        await this.walletRequestReview.rejectWithdrawal(rejection.request.id, dto);
      }
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reject the request.'));
    } finally {
      this.requests.reload();
      this.pending.set(false);
    }
  }
}
