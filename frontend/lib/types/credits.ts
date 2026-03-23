export interface WalletDto {
  userId: string;
  username: string;
  balance: number;
  totalSwapsCompleted: number;
  totalEarned: number;
  totalSpent: number;
}

export interface CreditTransactionDto {
  id: number | string;
  transactionId?: number | string;
  userId: string;
  type: string | number;
  typeLabel?: string;
  amount: number;
  balanceAfter: number;
  description?: string;
  transactionDate: string;
  referenceId?: number | null;
  referenceType?: string | null;
}

export interface TransactionFilterDto {
  type?: string;
  page: number;
  pageSize: number;
}

export interface TransactionListResponseDto {
  transactions: CreditTransactionDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LeaderboardEntryDto {
  rank: number;
  userId: string;
  username: string;
  profilePictureUrl?: string | null;
  creditBalance: number;
  completedSwaps: number;
  badgeCount: number;
}

export interface LeaderboardResponseDto {
  entries: LeaderboardEntryDto[];
  currentUserRank: number | null;
  currentUserBalance: number | null;
}

export interface CreditPackDto {
  id: string;
  name: string;
  credits: number;
  priceUsd: number;
  badgeLabel?: string | null;
}

export interface CheckoutSessionDto {
  sessionId: string;
  url: string;
}

export interface ServiceResponse<T> {
  success: boolean;
  message: string;
  data: T;
}
