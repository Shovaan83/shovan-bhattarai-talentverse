"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { X, RefreshCw, Loader2 } from "lucide-react";
import type { UseMutationResult } from "@tanstack/react-query";
import type { Appointment, UpdateAppointmentPayload } from "@/lib/types/appointments";

const DURATION_OPTIONS = [30, 60, 90, 120] as const;

function toLocalDatetimeString(utcIso: string): string {
  const d = new Date(utcIso);
  return new Date(d.getTime() - d.getTimezoneOffset() * 60_000)
    .toISOString()
    .slice(0, 16);
}

function minDatetimeLocal(): string {
  const now = new Date(Date.now() + 15 * 60 * 1000);
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000)
    .toISOString()
    .slice(0, 16);
}

const schema = z.object({
  meetingTime: z.string().min(1, "Please choose a date and time"),
  duration: z.number()
    .refine(
      (v) => DURATION_OPTIONS.includes(v as typeof DURATION_OPTIONS[number]),
      { message: "Select a valid duration" }
    ),
  description: z.string().max(500, "Max 500 characters").optional(),
});

type FormValues = z.infer<typeof schema>;

interface Props {
  appointment: Appointment;
  proposalId: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  rescheduleMutation: UseMutationResult<any, Error, { id: number; payload: UpdateAppointmentPayload }, unknown>;
  onClose: () => void;
}

export default function RescheduleModal({ appointment, rescheduleMutation, onClose }: Props) {
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      meetingTime: toLocalDatetimeString(appointment.meetingTime),
      duration: appointment.duration,
      description: appointment.description ?? "",
    },
  });

  const onSubmit = async (values: FormValues) => {
    const result = await rescheduleMutation.mutateAsync({
      id: appointment.appointmentId,
      payload: {
        meetingTime: new Date(values.meetingTime).toISOString(),
        duration: values.duration,
        description: values.description || undefined,
      },
    });
    if (result.success) onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-2xl overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <div className="flex items-center gap-2">
            <RefreshCw size={16} className="text-blue-600" />
            <h2 className="font-heading font-bold text-gray-900">Reschedule Meeting</h2>
          </div>
          <button onClick={onClose} className="p-1 text-gray-400 hover:text-gray-600 rounded-lg transition-colors">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-5">
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              New Date &amp; Time <span className="text-gray-400 font-normal">(your local time)</span>
            </label>
            <input
              type="datetime-local"
              min={minDatetimeLocal()}
              {...register("meetingTime")}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.meetingTime && (
              <p className="text-xs text-red-600">{errors.meetingTime.message}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Duration</label>
            <select
              {...register("duration", { valueAsNumber: true })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {DURATION_OPTIONS.map((mins) => (
                <option key={mins} value={mins}>{mins} minutes</option>
              ))}
            </select>
          </div>

          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Description <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              {...register("description")}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.description && (
              <p className="text-xs text-red-600">{errors.description.message}</p>
            )}
          </div>

          {rescheduleMutation.isError && (
            <p className="text-xs text-red-600 bg-red-50 rounded-lg px-3 py-2">
              {(rescheduleMutation.error as Error)?.message ?? "Failed to reschedule."}
            </p>
          )}

          <div className="flex gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 text-sm font-medium rounded-xl hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={rescheduleMutation.isPending}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold rounded-xl disabled:opacity-50"
            >
              {rescheduleMutation.isPending ? (
                <Loader2 size={14} className="animate-spin" />
              ) : (
                <RefreshCw size={14} />
              )}
              Reschedule
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
