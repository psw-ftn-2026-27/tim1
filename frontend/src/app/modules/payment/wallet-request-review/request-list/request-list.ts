import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { formatAmount } from '../../../../shared/util/format-amount';
import { WalletRequestReviewDto, WalletRequestStatus } from '../../api/payment-api-types';

export interface RequestRejection {
  request: WalletRequestReviewDto;
  reason: string;
}

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

@Component({
  imports: [FormField],
  selector: 'app-request-list',
  styleUrl: './request-list.scss',
  templateUrl: './request-list.html',
})
export class RequestList {
  readonly requests = input.required<WalletRequestReviewDto[]>();
  readonly decidable = input(false);
  readonly pending = input(false);

  readonly approveRequest = output<WalletRequestReviewDto>();
  readonly rejectRequest = output<RequestRejection>();

  protected readonly rejectingId = signal<string | null>(null);

  protected readonly rejectForm = form(signal({ reason: '' }), (path) => {
    required(path.reason, { message: 'A reason is required.' });
  });

  protected readonly formatAmount = formatAmount;

  protected statusLabel(status: WalletRequestStatus): string {
    return STATUS_LABELS[status];
  }

  protected statusBadge(status: WalletRequestStatus): string {
    return STATUS_BADGES[status];
  }

  protected startReject(request: WalletRequestReviewDto): void {
    this.rejectingId.set(request.id);
    this.rejectForm().reset({ reason: '' });
  }

  protected submitReject(event: Event, request: WalletRequestReviewDto): void {
    event.preventDefault();
    this.rejectRequest.emit({ request, reason: this.rejectForm.reason().value() });
    this.rejectingId.set(null);
    this.rejectForm().reset({ reason: '' });
  }
}
