"use client";

import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useRef } from 'react';
import toast from 'react-hot-toast';
import { proposalsApi } from '@/lib/api/proposals';
import { PROPOSALS_QUERY_KEY } from '@/lib/hooks/useProposals';
import type { ProposalListItem, ProposalListResponse } from '@/lib/types/proposals';

const HUB_URL =
  (process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5249/api').replace(/\/api$/, '') + '/hubs/chat';

type ProposalRealtimeEvent = {
  proposalId: number;
  proposerId: string;
  recipientId: string;
  proposerUsername: string;
  recipientUsername: string;
  proposerProfilePicture?: string;
  recipientProfilePicture?: string;
  offeringSkillName: string;
  receivingSkillName: string;
  creditAmount: number;
  eventType: string;
  status: string;
  title: string;
  message: string;
  actorUserId: string;
  actorUsername?: string;
  occurredAt: string;
};

/**
 * Hook to fetch pending proposal notifications count and listen for real-time updates.
 */
export function useProposalNotifications(currentUserId?: string): { count: number; isLoading: boolean } {
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  const updateNotificationCaches = (event: ProposalRealtimeEvent) => {
    const isRecipient = currentUserId != null && event.recipientId === currentUserId;

    queryClient.setQueryData<ProposalListResponse>(['proposals', 'notifications'], (old) => {
      if (!old) return old;

      if (event.eventType === 'Created' && isRecipient) {
        return {
          ...old,
          totalCount: old.totalCount + 1,
        };
      }

      if (event.eventType !== 'Created' && isRecipient && old.totalCount > 0) {
        return {
          ...old,
          totalCount: old.totalCount - 1,
        };
      }

      return old;
    });

    queryClient.setQueryData<ProposalListResponse>(['proposals', 'notifications', 'recent'], (old) => {
      if (!old) return old;

      if (event.eventType === 'Created' && isRecipient) {
        const nextItem: ProposalListItem = {
          proposalId: event.proposalId,
          creditAmount: event.creditAmount,
          otherUserId: event.proposerId,
          otherUsername: event.proposerUsername,
          otherProfilePicture: event.proposerProfilePicture,
          offeringSkillName: event.offeringSkillName,
          receivingSkillName: event.receivingSkillName,
          status: event.status as ProposalListItem['status'],
          proposerConfirmed: false,
          recipientConfirmed: false,
          isProposer: false,
          createdAt: event.occurredAt,
          updatedAt: event.occurredAt,
        };

        const existing = old.proposals.filter((proposal) => proposal.proposalId !== event.proposalId);
        return {
          ...old,
          proposals: [nextItem, ...existing].slice(0, old.pageSize),
          totalCount: old.totalCount + 1,
        };
      }

      if (event.eventType !== 'Created') {
        const filtered = old.proposals.filter((proposal) => proposal.proposalId !== event.proposalId);
        if (filtered.length === old.proposals.length) return old;

        return {
          ...old,
          proposals: filtered,
          totalCount: Math.max(0, old.totalCount - 1),
        };
      }

      return old;
    });
  };

  useEffect(() => {
    if (!currentUserId) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => (typeof window !== 'undefined' ? localStorage.getItem('token') ?? '' : ''),
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('ProposalActivityUpdated', (event: ProposalRealtimeEvent) => {
      updateNotificationCaches(event);
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['proposals', 'notifications'] });
      queryClient.invalidateQueries({ queryKey: ['proposals', 'notifications', 'recent'] });

      if (!currentUserId || event.actorUserId !== currentUserId) {
        toast.success(`${event.title}: ${event.message}`);
      }
    });

    connection.onreconnected(() => {
      queryClient.invalidateQueries({ queryKey: PROPOSALS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['proposals', 'notifications'] });
      queryClient.invalidateQueries({ queryKey: ['proposals', 'notifications', 'recent'] });
    });

    let cancelled = false;
    connection.start().catch(() => {
      if (!cancelled) {
        connectionRef.current = null;
      }
    });

    return () => {
      cancelled = true;
      connection.off('ProposalActivityUpdated');
      if (
        connection.state === HubConnectionState.Connected ||
        connection.state === HubConnectionState.Reconnecting
      ) {
        connection.stop().catch(() => {});
      }
    };
  }, [currentUserId, queryClient]);

  const { data, isLoading } = useQuery({
    queryKey: ['proposals', 'notifications'],
    queryFn: () => proposalsApi.getProposals({
      direction: 'received',
      status: 'Pending',
      page: 1,
      pageSize: 1, // Only need count, not actual data
    }),
    staleTime: 30000, // 30 seconds
  });

  return {
    count: data?.totalCount || 0,
    isLoading,
  };
}
