import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { appointmentsApi } from "@/lib/api/appointments";
import type { CreateAppointmentPayload, UpdateAppointmentPayload } from "@/lib/types/appointments";

export const CALENDAR_STATUS_KEY = ["googleCalendarStatus"] as const;
export const APPOINTMENTS_KEY = ["appointments"] as const;

// ---- Google Calendar status ----

export function useGoogleCalendarStatus() {
  return useQuery({
    queryKey: CALENDAR_STATUS_KEY,
    queryFn: appointmentsApi.getCalendarStatus,
    staleTime: 30_000,
  });
}

export function useDisconnectCalendar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: appointmentsApi.disconnectCalendar,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CALENDAR_STATUS_KEY });
    },
  });
}

// ---- Appointments ----

export function useProposalAppointments(proposalId: number) {
  return useQuery({
    queryKey: [...APPOINTMENTS_KEY, "proposal", proposalId],
    queryFn: () => appointmentsApi.getProposalAppointments(proposalId),
    enabled: proposalId > 0,
  });
}

export function useAppointment(id: number) {
  return useQuery({
    queryKey: [...APPOINTMENTS_KEY, id],
    queryFn: () => appointmentsApi.getAppointment(id),
    enabled: id > 0,
  });
}

export function useCreateAppointment(proposalId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateAppointmentPayload) =>
      appointmentsApi.createAppointment(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [...APPOINTMENTS_KEY, "proposal", proposalId],
      });
    },
  });
}

export function useCancelAppointment(proposalId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number) => appointmentsApi.cancelAppointment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [...APPOINTMENTS_KEY, "proposal", proposalId],
      });
    },
  });
}

export function useRescheduleAppointment(proposalId: number) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateAppointmentPayload }) =>
      appointmentsApi.rescheduleAppointment(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [...APPOINTMENTS_KEY, "proposal", proposalId],
      });
    },
  });
}
