import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { formatAmount } from '../../../../shared/util/format-amount';
import { serverMessage } from '../../../../shared/util/server-message';
import { TransactionType, WalletOverviewDto, WalletRequestStatus } from '../../api/payment-api-types';
import { WalletFunding } from '../wallet-funding';

const STATUS_LABELS: Record<WalletRequestStatus, string> = {
  Pending: 'na čekanju',
  Approved: 'odobren',
  Rejected: 'odbijen',
};

const STATUS_BADGES: Record<WalletRequestStatus, string> = {
  Pending: 'badge--warning',
  Approved: 'badge--success',
  Rejected: 'badge--danger',
};

// Server šalje iznos bez predznaka; smer novca u odnosu na raspoloživo stanje zavisi od vrste.
// Isplata (Payout) ide iz rezervisanog, pa raspoloživo stanje ne menja.
const TRANSACTION_DIRECTIONS: Record<TransactionType, 'in' | 'out' | 'reserved'> = {
  TopUp: 'in',
  Sale: 'in',
  ReservationReturn: 'in',
  Reservation: 'out',
  Purchase: 'out',
  GiftCardPurchase: 'out',
  Payout: 'reserved',
};

@Component({
  imports: [FormField, RouterLink, RouterLinkActive],
  selector: 'app-wallet',
  styleUrl: './wallet.scss',
  templateUrl: './wallet.html',
})
export class Wallet {
  private readonly walletFunding = inject(WalletFunding);
  private readonly auth = inject(Auth);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );

  protected readonly wallet = httpResource<WalletOverviewDto>(() => '/api/payment/wallet');

  protected readonly validReceivedCount = httpResource<number>(() =>
    this.isAdministrator() ? undefined : '/api/payment/gift-cards/received/valid-count',
  );

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

  protected readonly formatAmount = formatAmount;

  protected statusLabel(status: WalletRequestStatus): string {
    return STATUS_LABELS[status];
  }

  protected statusBadge(status: WalletRequestStatus): string {
    return STATUS_BADGES[status];
  }

  protected direction(type: TransactionType): 'in' | 'out' | 'reserved' {
    return TRANSACTION_DIRECTIONS[type];
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
