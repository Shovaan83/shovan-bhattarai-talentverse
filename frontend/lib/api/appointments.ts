import axiosInstance from "@/lib/axios";
import type {
  Appointment,
  CreateAppointmentPayload,
  UpdateAppointmentPayload,
  GoogleCalendarStatus,
} from "@/lib/types/appointments";

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors?: string[];
}

export const appointmentsApi = {
  // --- Google Calendar OAuth ---

  getCalendarStatus: async (): Promise<GoogleCalendarStatus> => {
    const response = await axiosInstance.get<ServiceResponse<GoogleCalendarStatus>>(
      "/appointments/google-calendar/status"
    );
    return response.data.data;
  },

  // Fetches the Google OAuth URL from the backend (requires JWT), then
  // the caller does window.location.href = url to start the OAuth flow.
  getConnectUrl: async (): Promise<string> => {
    const response = await axiosInstance.get<ServiceResponse<string>>(
      "/appointments/google-calendar/connect"
    );
    return response.data.data;
  },

  disconnectCalendar: async (): Promise<ServiceResponse<object>> => {
    const response = await axiosInstance.delete<ServiceResponse<object>>(
      "/appointments/google-calendar/disconnect"
    );
    return response.data;
  },

  // --- Appointments CRUD ---

  createAppointment: async (
    payload: CreateAppointmentPayload
  ): Promise<ServiceResponse<Appointment>> => {
    const response = await axiosInstance.post<ServiceResponse<Appointment>>(
      "/appointments",
      payload
    );
    return response.data;
  },

  getProposalAppointments: async (proposalId: number): Promise<Appointment[]> => {
    const response = await axiosInstance.get<ServiceResponse<Appointment[]>>(
      `/appointments/proposal/${proposalId}`
    );
    return response.data.data;
  },

  getAppointment: async (id: number): Promise<Appointment> => {
    const response = await axiosInstance.get<ServiceResponse<Appointment>>(
      `/appointments/${id}`
    );
    return response.data.data;
  },

  cancelAppointment: async (id: number): Promise<ServiceResponse<Appointment>> => {
    const response = await axiosInstance.patch<ServiceResponse<Appointment>>(
      `/appointments/${id}/cancel`
    );
    return response.data;
  },

  rescheduleAppointment: async (
    id: number,
    payload: UpdateAppointmentPayload
  ): Promise<ServiceResponse<Appointment>> => {
    const response = await axiosInstance.put<ServiceResponse<Appointment>>(
      `/appointments/${id}`,
      payload
    );
    return response.data;
  },
};
