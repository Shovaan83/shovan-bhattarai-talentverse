import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { proposalsApi } from "@/lib/api/proposals";
import type { ProposalFilter, ProposalListItem, CreateCounterofferPayload } from "@/lib/types/proposals";

export const PROPOSALS_QUERY_KEY = ["proposals"] as const;

// Hook to fetch paginated proposals
export function useProposals(filter?: ProposalFilter) {
  return useQuery({
    queryKey: [...PROPOSALS_QUERY_KEY, filter],
    queryFn: () => proposalsApi.getProposals(filter),
  });
}

// Hook to fetch a single proposal
export function useProposal(id: number) {
  return useQuery({
    queryKey: [...PROPOSALS_QUERY_KEY, id],
    queryFn: () => proposalsApi.getProposal(id),
    enabled: id > 0,
  });
}

// Hook to create a proposal
export function useCreateProposal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: proposalsApi.createProposal,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}

// Hook to submit a counteroffer
export function useCounterofferProposal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ proposalId, payload }: { proposalId: number; payload: CreateCounterofferPayload }) =>
      proposalsApi.counterofferProposal(proposalId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}

// Hook to accept a proposal
export function useAcceptProposal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: proposalsApi.acceptProposal,
    onMutate: async (proposalId) => {
      await queryClient.cancelQueries({ queryKey: PROPOSALS_QUERY_KEY });
      return { proposalId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}

// Hook to decline a proposal
export function useDeclineProposal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: proposalsApi.declineProposal,
    onMutate: async (proposalId) => {
      await queryClient.cancelQueries({ queryKey: PROPOSALS_QUERY_KEY });
      return { proposalId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}

// Hook to cancel a proposal
export function useCancelProposal() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: proposalsApi.cancelProposal,
    onMutate: async (proposalId) => {
      await queryClient.cancelQueries({ queryKey: PROPOSALS_QUERY_KEY });
      
      // Optimistic update - mark as cancelled in UI
      queryClient.setQueriesData<{ proposals: ProposalListItem[] }>(
        { queryKey: PROPOSALS_QUERY_KEY },
        (old) => {
          if (!old) return old;
          return {
            ...old,
            proposals: old.proposals.map((p) =>
              p.proposalId === proposalId ? { ...p, status: "Cancelled" as const } : p
            ),
          };
        }
      );
      
      return { proposalId };
    },
    onError: (_err, _proposalId, _context) => {
      // Revert on error by invalidating
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}

// Hook to confirm completion
export function useConfirmCompletion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: proposalsApi.confirmCompletion,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
    },
  });
}
