export type TransactionType =
  | 'TopUp'
  | 'Reservation'
  | 'Payout'
  | 'ReservationReturn'
  | 'Purchase'
  | 'Sale'
  | 'GiftCardPurchase';

export type WalletRequestType = 'TopUp' | 'Withdrawal';

export type WalletRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface WalletTransactionDto {
  occurredAt: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
  productName: string | null;
}

export interface WalletRequestDto {
  id: string;
  type: WalletRequestType;
  amount: number;
  status: WalletRequestStatus;
  createdAt: string;
  rejectionReason: string | null;
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

export interface WalletRequestReviewDto {
  id: string;
  type: WalletRequestType;
  submitterId: string;
  amount: number;
  createdAt: string;
  status: WalletRequestStatus;
  rejectionReason: string | null;
}

export interface WalletRequestsDto {
  pending: WalletRequestReviewDto[];
  decided: WalletRequestReviewDto[];
}

export interface RejectRequestDto {
  reason: string;
}

export type ProductType = 'Tour';

export interface ProductDto {
  id: string;
  type: ProductType;
  entityId: string;
  name: string;
  price: number;
  ownerId: string;
  ownerName: string;
  purchased: boolean;
  inCart: boolean;
}

export interface CartItemDto {
  type: ProductType;
  entityId: string;
  name: string;
  price: number;
}

export interface CartDto {
  items: CartItemDto[];
  total: number;
}

export interface ProductReferenceDto {
  type: ProductType;
  entityId: string;
}

export type GiftCardStatus = 'Valid' | 'Used' | 'Expired';

export interface GiftCardDto {
  id: string;
  creatorId: string;
  creatorDisplayName: string;
  recipientId: string;
  recipientDisplayName: string;
  amount: number;
  message: string;
  expiresAt: string;
  status: GiftCardStatus;
}

export interface PurchaseGiftCardDto {
  recipientId: string;
  amount: number;
  message: string;
}

export interface RecipientDto {
  id: string;
  displayName: string;
}

export interface CouponDto {
  id: string;
  code: string;
  percent: number;
  expiresAt: string;
  productName: string | null;
  used: boolean;
}

export interface OwnedProductDto {
  id: string;
  name: string;
}

export interface CreateCouponDto {
  productId: string | null;
  percent: number;
  expiresAt: string;
}
