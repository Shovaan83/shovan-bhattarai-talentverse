"use client";

import { format } from "date-fns";
import { Calendar, Clock, ExternalLink, Loader2, RefreshCw, X } from "lucide-react";
import { useState } from "react";
import { useCancelAppointment, useRescheduleAppointment } from "@/lib/hooks/useAppointments";
import type { Appointment } from "@/lib/types/appointments";
import RescheduleModal from "./RescheduleModal";

const statusBadge: Record<
  string,
  { label: string; className: string }
> = {
  Scheduled: { label: "Scheduled", className: "bg-emerald-100 text-emerald-700" },
  Rescheduled: { label: "Rescheduled", className: "bg-amber-100 text-amber-700" },
  Cancelled: { label: "Cancelled", className: "bg-red-100 text-red-600 line-through" },
};

interface Props {
  appointment: Appointment;
  proposalId: number;
}

export default function AppointmentCard({ appointment, proposalId }: Props) {
  const [showReschedule, setShowReschedule] = useState(false);
  const cancelMutation = useCancelAppointment(proposalId);
  const rescheduleMutation = useRescheduleAppointment(proposalId);

  // Convert UTC ISO → local Date for display
  const localDate = new Date(appointment.meetingTime);
  const badge = statusBadge[appointment.status] ?? { label: appointment.status, className: "bg-gray-100 text-gray-600" };
  const isCancelled = appointment.status === "Cancelled";

  return (
    <>
      <div
        className={`bg-white rounded-2xl border p-5 space-y-4 shadow-sm ${isCancelled ? "opacity-60" : ""}`}
      >
        {/* Header row */}
        <div className="flex items-start justify-between gap-2">
          <div className="space-y-1">
            <div className="flex items-center gap-2">
              <Calendar size={15} className="text-emerald-600 shrink-0" />
              <span className="font-semibold text-gray-900 text-sm">
                {format(localDate, "EEEE, MMMM d, yyyy")}
              </span>
            </div>
            <div className="flex items-center gap-2 text-gray-500">
              <Clock size={13} className="shrink-0" />
              <span className="text-xs">
                {format(localDate, "h:mm a")} · {appointment.duration} min
              </span>
            </div>
          </div>
          <span className={`text-xs font-medium px-2 py-1 rounded-full shrink-0 ${badge.className}`}>
            {badge.label}
          </span>
        </div>

        {/* Description */}
        {appointment.description && (
          <p className="text-sm text-gray-600 leading-relaxed">{appointment.description}</p>
        )}

        {/* Google Meet link */}
        {appointment.meetingLink && !isCancelled && (
          <a
            href={appointment.meetingLink}
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            <ExternalLink size={13} />
            Join Google Meet
          </a>
        )}

        {/* Scheduled by */}
        <p className="text-xs text-gray-400">
          Scheduled by {appointment.createdByUsername}
        </p>

        {/* Actions */}
        {!isCancelled && (appointment.canReschedule || appointment.canCancel) && (
          <div className="flex gap-2 pt-1 border-t border-gray-100">
            {appointment.canReschedule && (
              <button
                onClick={() => setShowReschedule(true)}
                className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-700 font-medium px-3 py-1.5 rounded-lg hover:bg-blue-50 transition-colors"
              >
                <RefreshCw size={12} />
                Reschedule
              </button>
            )}
            {appointment.canCancel && (
              <button
                onClick={() => cancelMutation.mutate(appointment.appointmentId)}
                disabled={cancelMutation.isPending}
                className="flex items-center gap-1 text-xs text-red-600 hover:text-red-700 font-medium px-3 py-1.5 rounded-lg hover:bg-red-50 transition-colors disabled:opacity-50"
              >
                {cancelMutation.isPending ? (
                  <Loader2 size={12} className="animate-spin" />
                ) : (
                  <X size={12} />
                )}
                Cancel
              </button>
            )}
          </div>
        )}
      </div>

      {showReschedule && (
        <RescheduleModal
          appointment={appointment}
          proposalId={proposalId}
          rescheduleMutation={rescheduleMutation}
          onClose={() => setShowReschedule(false)}
        />
      )}
    </>
  );
}
