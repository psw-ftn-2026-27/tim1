export type TransactionType = 'TopUp';

export type WalletRequestType = 'TopUp';

export type TopUpRequestStatus = 'Pending' | 'Approved' | 'Rejected';

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
  status: TopUpRequestStatus;
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
