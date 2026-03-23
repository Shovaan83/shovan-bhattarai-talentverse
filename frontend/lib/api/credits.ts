import axiosInstance from "../axios";
import type {
  WalletDto,
  CreditTransactionDto,
  CreditPackDto,
  CheckoutSessionDto,
  TransactionFilterDto,
  TransactionListResponseDto,
  LeaderboardResponseDto,
  ServiceResponse,
} from "../types/credits";

export const creditsApi = {
  /**
   * Get the current user's wallet summary
   */
  getWallet: async (): Promise<WalletDto> => {
    const response = await axiosInstance.get<ServiceResponse<WalletDto>>(
      "/credits/wallet"
    );
    return response.data.data;
  },

  /**
   * Get paginated transaction history for the current user
   */
  getTransactions: async (
    filter: TransactionFilterDto
  ): Promise<TransactionListResponseDto> => {
    const params: Record<string, string | number> = {
      page: filter.page,
      pageSize: filter.pageSize,
    };
    if (filter.type) params.type = filter.type;

    const response = await axiosInstance.get<
      ServiceResponse<TransactionListResponseDto>
    >("/credits/transactions", { params });

    // Backend uses TransactionId while frontend historically reads id.
    // Normalize here so rendering keys are always stable.
    const normalizedTransactions = (response.data.data.transactions ?? []).map(
      (tx, index): CreditTransactionDto => {
        const normalizedId = tx.id ?? tx.transactionId ?? index;
        const normalizedType =
          typeof tx.type === "number"
            ? tx.typeLabel ?? String(tx.type)
            : tx.type;

        return {
          ...tx,
          id: normalizedId,
          transactionId: tx.transactionId ?? normalizedId,
          type: normalizedType,
          description: tx.description ?? "Transaction",
        };
      }
    );

    return {
      ...response.data.data,
      transactions: normalizedTransactions,
    };
  },

  /**
   * Get the credits leaderboard
   */
  getLeaderboard: async (): Promise<LeaderboardResponseDto> => {
    const response = await axiosInstance.get<
      ServiceResponse<LeaderboardResponseDto>
    >("/credits/leaderboard");
    return response.data.data;
  },

  /**
   * Get available credit purchase packs
   */
  getCreditPacks: async (): Promise<CreditPackDto[]> => {
    const response = await axiosInstance.get<ServiceResponse<CreditPackDto[]>>(
      "/credits/packs"
    );
    return response.data.data;
  },

  /**
   * Create a Stripe checkout session for purchasing credits
   */
  createCheckoutSession: async (
    packId: string,
    successUrl: string,
    cancelUrl: string
  ): Promise<CheckoutSessionDto> => {
    const response = await axiosInstance.post<
      ServiceResponse<CheckoutSessionDto>
    >("/credits/checkout", { packId, successUrl, cancelUrl });
    return response.data.data;
  },
};
