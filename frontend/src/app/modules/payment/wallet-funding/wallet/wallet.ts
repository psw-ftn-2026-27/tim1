import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { TopUpRequestStatus, WalletOverviewDto } from '../../api/payment-api-types';
import { WalletFunding } from '../wallet-funding';

const STATUS_LABELS: Record<TopUpRequestStatus, string> = {
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ amount: 0 });

  protected readonly form = form(this.model, (path) => {
    required(path.amount, { message: 'Amount is required.' });
  });

  protected statusLabel(status: TopUpRequestStatus): string {
    return STATUS_LABELS[status];
  }

  protected async requestTopUp(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.walletFunding.requestTopUp({ amount: this.form.amount().value() });
      this.wallet.reload();
      this.form().reset({ amount: 0 });
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not request a top-up.'));
    } finally {
      this.pending.set(false);
    }
  }
}
