"use client";

import { AlertTriangle, Calendar, CheckCircle2, Loader2, LogOut } from "lucide-react";
import { useState } from "react";
import { useGoogleCalendarStatus, useDisconnectCalendar } from "@/lib/hooks/useAppointments";
import { appointmentsApi } from "@/lib/api/appointments";

export default function ConnectGoogleCalendar() {
  const { data: status, isLoading } = useGoogleCalendarStatus();
  const disconnectMutation = useDisconnectCalendar();
  const [isConnecting, setIsConnecting] = useState(false);

  const handleConnect = async () => {
    setIsConnecting(true);
    try {
      const url = await appointmentsApi.getConnectUrl();
      window.location.href = url;
    } catch {
      setIsConnecting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center gap-2 text-sm text-gray-500 py-2">
        <Loader2 size={14} className="animate-spin" />
        Checking calendar…
      </div>
    );
  }

  if (status?.isConnected && !status?.isRevoked) {
    return (
      <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4">
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <CheckCircle2 size={16} className="text-emerald-600 shrink-0" />
            <div>
              <p className="text-sm font-medium text-emerald-800">Google Calendar connected</p>
              {status.googleEmail && (
                <p className="text-xs text-emerald-600">{status.googleEmail}</p>
              )}
            </div>
          </div>
          <button
            onClick={() => disconnectMutation.mutate()}
            disabled={disconnectMutation.isPending}
            className="flex items-center gap-1 text-xs text-red-600 hover:text-red-700 disabled:opacity-50"
          >
            {disconnectMutation.isPending ? (
              <Loader2 size={12} className="animate-spin" />
            ) : (
              <LogOut size={12} />
            )}
            Disconnect
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-xl border bg-white p-4 space-y-3">
      {status?.isRevoked && (
        <div className="flex items-start gap-2 text-amber-700 bg-amber-50 rounded-lg p-3">
          <AlertTriangle size={14} className="mt-0.5 shrink-0" />
          <p className="text-xs">
            Your Google Calendar access was revoked. Please reconnect to schedule meetings.
          </p>
        </div>
      )}
      <div className="flex items-center gap-2">
        <Calendar size={16} className="text-gray-500 shrink-0" />
        <p className="text-sm text-gray-700">
          Connect Google Calendar to schedule meetings with Google Meet links.
        </p>
      </div>
      <button
        onClick={handleConnect}
        disabled={isConnecting}
        className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors disabled:opacity-50"
      >
        {isConnecting ? (
          <Loader2 size={14} className="animate-spin" />
        ) : (
          <Calendar size={14} />
        )}
        {status?.isRevoked ? "Reconnect Google Calendar" : "Connect Google Calendar"}
      </button>
    </div>
  );
}
