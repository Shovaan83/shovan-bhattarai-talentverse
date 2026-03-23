"use client";

import { CalendarX, Loader2 } from "lucide-react";
import { useProposalAppointments } from "@/lib/hooks/useAppointments";
import AppointmentCard from "./AppointmentCard";

interface Props {
  proposalId: number;
}

export default function AppointmentsList({ proposalId }: Props) {
  const { data: appointments, isLoading, isError } = useProposalAppointments(proposalId);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8 text-emerald-200">
        <Loader2 size={20} className="animate-spin mr-2" />
        Loading appointments…
      </div>
    );
  }

  if (isError) {
    return (
      <p className="text-center text-red-400 py-4 text-sm">
        Failed to load appointments.
      </p>
    );
  }

  if (!appointments || appointments.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 py-8 text-emerald-300">
        <CalendarX size={32} className="opacity-50" />
        <p className="text-sm">No meetings scheduled yet.</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {appointments.map((appt) => (
        <AppointmentCard key={appt.appointmentId} appointment={appt} proposalId={proposalId} />
      ))}
    </div>
  );
}
