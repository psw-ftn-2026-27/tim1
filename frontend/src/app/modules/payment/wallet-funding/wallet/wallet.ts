import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { WalletOverviewDto, WalletRequestStatus } from '../../api/payment-api-types';
import { WalletFunding } from '../wallet-funding';

const STATUS_LABELS: Record<WalletRequestStatus, string> = {
  Pending: 'na čekanju',
  Approved: 'odobren',
  Rejected: 'odbijen',
};

@Component({
  imports: [FormField],
  selector: 'app-wallet',
  styleUrl: './wallet.scss',
  templateUrl: './wallet.html',
})
export class Wallet {
  private readonly walletFunding = inject(WalletFunding);

  protected readonly wallet = httpResource<WalletOverviewDto>(() => '/api/payment/wallet');

  protected readonly topUpPending = signal(false);
  protected readonly topUpError = signal<string | null>(null);

  protected readonly withdrawalPending = signal(false);
  protected readonly withdrawalError = signal<string | null>(null);

  private readonly topUpModel = signal({ amount: 0 });
  private readonly withdrawalModel = signal({ amount: 0 });

  protected readonly topUpForm = form(this.topUpModel, (path) => {
    required(path.amount, { message: 'Amount is required.' });
  });

  protected readonly withdrawalForm = form(this.withdrawalModel, (path) => {
    required(path.amount, { message: 'Amount is required.' });
  });

  protected statusLabel(status: WalletRequestStatus): string {
    return STATUS_LABELS[status];
  }

  protected async requestTopUp(event: Event): Promise<void> {
    event.preventDefault();
    this.topUpPending.set(true);
    this.topUpError.set(null);
    try {
      await this.walletFunding.requestTopUp({ amount: this.topUpForm.amount().value() });
      this.wallet.reload();
      this.topUpForm().reset({ amount: 0 });
    } catch (failure) {
      this.topUpError.set(serverMessage(failure, 'Could not request a top-up.'));
    } finally {
      this.topUpPending.set(false);
    }
  }

  protected async requestWithdrawal(event: Event): Promise<void> {
    event.preventDefault();
    this.withdrawalPending.set(true);
    this.withdrawalError.set(null);
    try {
      await this.walletFunding.requestWithdrawal({
        amount: this.withdrawalForm.amount().value(),
      });
      this.wallet.reload();
      this.withdrawalForm().reset({ amount: 0 });
    } catch (failure) {
      this.withdrawalError.set(serverMessage(failure, 'Could not request a withdrawal.'));
    } finally {
      this.withdrawalPending.set(false);
    }
  }
}
