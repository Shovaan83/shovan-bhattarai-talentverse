"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef, useState } from "react";
import { MESSAGE_QUERY_KEYS } from "@/lib/hooks/useMessages";
import type { Message } from "@/lib/types/messages";

const HUB_URL =
  (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5249/api").replace(
    /\/api$/,
    ""
  ) + "/hubs/chat";

export function useSignalR(proposalId?: number, currentUserId?: string) {
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const joinedGroupRef = useRef<number | null>(null);
  const currentUserIdRef = useRef(currentUserId);
  currentUserIdRef.current = currentUserId;

  // Build and start the hub connection once
  useEffect(() => {
    const token =
      typeof window !== "undefined" ? localStorage.getItem("token") : null;
    if (!token) return;

    const connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    // Listen for incoming messages
    connection.on("ReceiveMessage", (message: Message) => {
      if (!message) return;
      // Recompute isOwnMessage client-side: the backend sets it from the
      // sender's perspective, but the same DTO is broadcast to all group
      // members, so the recipient would receive isOwnMessage=true incorrectly.
      const corrected: Message = {
        ...message,
        isOwnMessage:
          currentUserIdRef.current != null
            ? message.senderId === currentUserIdRef.current
            : message.isOwnMessage,
      };
      // Append to the cached message list for this proposal
      queryClient.setQueryData(
        MESSAGE_QUERY_KEYS.messages(message.proposalId, 1),
        (old: { messages: Message[]; totalCount: number; page: number; pageSize: number; hasMore: boolean } | undefined) => {
          if (!old) return old;
          // Avoid duplicates
          const exists = old.messages.some(
            (m) => m.messageId === corrected.messageId
          );
          if (exists) return old;
          return {
            ...old,
            messages: [...old.messages, corrected],
            totalCount: old.totalCount + 1,
          };
        }
      );
      // Refresh conversations list
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.conversations() });
    });

    // Listen for read receipts
    connection.on("MessagesRead", () => {
      queryClient.invalidateQueries({ queryKey: MESSAGE_QUERY_KEYS.conversations() });
    });

    // Listen for unread count updates
    connection.on("UnreadCountUpdated", (count: number) => {
      queryClient.setQueryData(MESSAGE_QUERY_KEYS.unreadCount(), count);
    });

    connection.onreconnected(() => {
      setIsConnected(true);
      // Rejoin proposal group if we had one
      if (joinedGroupRef.current !== null) {
        connection
          .invoke("JoinProposal", joinedGroupRef.current)
          .catch(() => {});
      }
    });

    connection.onclose(() => setIsConnected(false));

    // Use a flag to handle the case where this effect is cleaned up
    // before the connection finishes negotiating (e.g. React StrictMode
    // double-invocation). Calling stop() while still in "Connecting" state
    // throws "The connection was stopped during negotiation."
    let cancelled = false;

    connection
      .start()
      .then(() => {
        if (cancelled) {
          // Unmounted while negotiating — stop cleanly now that we're connected
          connection.stop().catch(() => {});
        } else {
          setIsConnected(true);
        }
      })
      .catch(() => {
        if (!cancelled) setIsConnected(false);
      });

    return () => {
      cancelled = true;
      setIsConnected(false);
      // Only call stop() when past negotiation; if still Connecting the
      // start() promise above will stop it once it resolves.
      if (
        connection.state === HubConnectionState.Connected ||
        connection.state === HubConnectionState.Reconnecting
      ) {
        connection.stop().catch(() => {});
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Join / leave proposal group when proposalId changes
  useEffect(() => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== HubConnectionState.Connected) return;

    const prev = joinedGroupRef.current;

    if (prev !== null && prev !== proposalId) {
      connection.invoke("LeaveProposal", prev).catch(() => {});
    }

    if (proposalId !== undefined && proposalId > 0 && proposalId !== prev) {
      connection.invoke("JoinProposal", proposalId).catch(() => {});
      joinedGroupRef.current = proposalId;
    }

    return () => {
      if (proposalId !== undefined && proposalId > 0) {
        connection.invoke("LeaveProposal", proposalId).catch(() => {});
        joinedGroupRef.current = null;
      }
    };
  }, [proposalId, isConnected]);

  const sendMessage = useCallback(
    (dto: { proposalId: number; messageContent: string }) => {
      return connectionRef.current?.invoke("SendMessage", dto);
    },
    []
  );

  const markAsRead = useCallback((pid: number) => {
    return connectionRef.current?.invoke("MarkAsRead", pid);
  }, []);

  return { isConnected, sendMessage, markAsRead };
}
