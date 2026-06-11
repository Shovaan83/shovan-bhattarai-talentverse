import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { creditsApi } from "../api/credits";
import type { TransactionFilterDto } from "../types/credits";

export const CREDITS_QUERY_KEY = ["credits"] as const;

/**
 * Get the current user's wallet (balance, totals, swap count)
 */
export function useWallet(enabled = true) {
  return useQuery({
    queryKey: [...CREDITS_QUERY_KEY, "wallet"],
    queryFn: creditsApi.getWallet,
    enabled,
  });
}

/**
 * Get paginated transaction history for the current user
 */
export function useTransactions(filter: TransactionFilterDto) {
  return useQuery({
    queryKey: [...CREDITS_QUERY_KEY, "transactions", filter],
    queryFn: () => creditsApi.getTransactions(filter),
  });
}

/**
 * Get the credits leaderboard
 */
export function useLeaderboard() {
  return useQuery({
    queryKey: [...CREDITS_QUERY_KEY, "leaderboard"],
    queryFn: creditsApi.getLeaderboard,
  });
}

/**
 * Get available credit purchase packs
 */
export function useCreditPacks() {
  return useQuery({
    queryKey: [...CREDITS_QUERY_KEY, "packs"],
    queryFn: creditsApi.getCreditPacks,
    staleTime: Infinity, // Packs don't change at runtime
  });
}

/**
 * Mutation to initiate a Stripe checkout session
 */
export function useCreateCheckoutSession() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packId,
      successUrl,
      cancelUrl,
    }: {
      packId: string;
      successUrl: string;
      cancelUrl: string;
    }) => creditsApi.createCheckoutSession(packId, successUrl, cancelUrl),
    onSuccess: () => {
      // Invalidate wallet so it refreshes after a successful payment redirect
      queryClient.invalidateQueries({ queryKey: [...CREDITS_QUERY_KEY, "wallet"] });
    },
  });
}
