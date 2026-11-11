export type TransactionType = 'TopUp' | 'Reservation';

export type WalletRequestType = 'TopUp' | 'Withdrawal';

export type WalletRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface WalletTransactionDto {
  occurredAt: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
}

export interface WalletRequestDto {
  id: string;
  type: WalletRequestType;
  amount: number;
  status: WalletRequestStatus;
  createdAt: string;
}

export interface WalletOverviewDto {
  availableBalance: number;
  reservedBalance: number;
  transactions: WalletTransactionDto[];
  requests: WalletRequestDto[];
}

export interface RequestTopUpDto {
  amount: number;
}

export interface RequestWithdrawalDto {
  amount: number;
}
