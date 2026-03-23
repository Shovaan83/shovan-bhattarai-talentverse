"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { X, Calendar, Loader2 } from "lucide-react";
import { useCreateAppointment } from "@/lib/hooks/useAppointments";

const DURATION_OPTIONS = [30, 60, 90, 120] as const;

// Minimum datetime-local value = now, adjusted to local timezone offset
function minDatetimeLocal(): string {
  const now = new Date(Date.now() + 15 * 60 * 1000); // at least 15 min in future
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000)
    .toISOString()
    .slice(0, 16);
}

const schema = z.object({
  meetingTime: z.string().min(1, "Please choose a date and time"),
  duration: z.number()
    .refine((v) => DURATION_OPTIONS.includes(v as typeof DURATION_OPTIONS[number]), {
      message: "Select a valid duration",
    }),
  description: z.string().max(500, "Max 500 characters").optional(),
});

type FormValues = z.infer<typeof schema>;

interface Props {
  proposalId: number;
  onClose: () => void;
}

export default function ScheduleMeetingModal({ proposalId, onClose }: Props) {
  const createMutation = useCreateAppointment(proposalId);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { duration: 60 },
  });

  const onSubmit = async (values: FormValues) => {
    // Convert local datetime-local string to UTC ISO string
    const utcIso = new Date(values.meetingTime).toISOString();

    const result = await createMutation.mutateAsync({
      proposalId,
      meetingTime: utcIso,
      duration: values.duration,
      description: values.description || undefined,
    });

    if (result.success) {
      onClose();
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="w-full max-w-md bg-white rounded-2xl shadow-2xl overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <div className="flex items-center gap-2">
            <Calendar size={18} className="text-emerald-600" />
            <h2 className="font-heading font-bold text-gray-900">Schedule Meeting</h2>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-gray-400 hover:text-gray-600 rounded-lg transition-colors"
          >
            <X size={20} />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-5">
          {/* Date + time */}
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Date &amp; Time <span className="text-gray-400 font-normal">(your local time)</span>
            </label>
            <input
              type="datetime-local"
              min={minDatetimeLocal()}
              {...register("meetingTime")}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
            {errors.meetingTime && (
              <p className="text-xs text-red-600">{errors.meetingTime.message}</p>
            )}
          </div>

          {/* Duration */}
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">Duration</label>
            <select
              {...register("duration", { valueAsNumber: true })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-emerald-500"
            >
              {DURATION_OPTIONS.map((mins) => (
                <option key={mins} value={mins}>{mins} minutes</option>
              ))}
            </select>
            {errors.duration && (
              <p className="text-xs text-red-600">{errors.duration.message}</p>
            )}
          </div>

          {/* Description */}
          <div className="space-y-1">
            <label className="block text-sm font-medium text-gray-700">
              Description <span className="text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              {...register("description")}
              rows={3}
              placeholder="What will you cover in this session?"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm resize-none focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
            {errors.description && (
              <p className="text-xs text-red-600">{errors.description.message}</p>
            )}
          </div>

          {/* Error from API */}
          {createMutation.isError && (
            <p className="text-xs text-red-600 bg-red-50 rounded-lg px-3 py-2">
              {(createMutation.error as Error)?.message ?? "Failed to schedule meeting."}
            </p>
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 text-sm font-medium rounded-xl hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold rounded-xl transition-colors disabled:opacity-50"
            >
              {createMutation.isPending ? (
                <Loader2 size={14} className="animate-spin" />
              ) : (
                <Calendar size={14} />
              )}
              Schedule
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
