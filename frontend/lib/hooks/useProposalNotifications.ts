import { useQuery } from '@tanstack/react-query';
import { proposalsApi } from '@/lib/api/proposals';

/**
 * Hook to fetch pending proposal notifications count
 * Used for notification badge on Proposals nav link and notification bell
 */
export function useProposalNotifications() {
  const { data, isLoading } = useQuery({
    queryKey: ['proposals', 'notifications'],
    queryFn: () => proposalsApi.getProposals({
      direction: 'received',
      status: 'Pending',
      page: 1,
      pageSize: 1, // Only need count, not actual data
    }),
    staleTime: 30000, // 30 seconds
    refetchInterval: 60000, // Refetch every minute
  });

  return {
    count: data?.totalCount || 0,
    isLoading,
  };
}
