export type AppointmentStatus = "Scheduled" | "Cancelled" | "Rescheduled";

export interface Appointment {
  appointmentId: number;
  proposalId: number;
  createdByUserId: string;
  createdByUsername: string;
  meetingTime: string; // ISO UTC string — convert to local on display
  duration: number; // minutes
  description?: string;
  meetingLink?: string;
  status: AppointmentStatus;
  createdAt: string;
  updatedAt: string;
  canCancel: boolean;
  canReschedule: boolean;
}

export interface CreateAppointmentPayload {
  proposalId: number;
  meetingTime: string; // ISO UTC string
  duration: number;
  description?: string;
}

export interface UpdateAppointmentPayload {
  meetingTime: string; // ISO UTC string
  duration: number;
  description?: string;
}

export interface GoogleCalendarStatus {
  isConnected: boolean;
  googleEmail?: string;
  isRevoked: boolean;
}
