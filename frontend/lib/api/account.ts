import axiosInstance from "@/lib/axios";
import type { CurrentUser } from "@/lib/types/account";
import type { UpdateProfilePayload } from "@/lib/types/account";

interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors?: string[];
}

export const accountApi = {
  getMe: async (): Promise<CurrentUser> => {
    const response = await axiosInstance.get<ServiceResponse<CurrentUser>>(
      "/account/me"
    );
    return response.data.data;
  },

  updateMe: async (payload: UpdateProfilePayload): Promise<CurrentUser> => {
    const response = await axiosInstance.put<ServiceResponse<CurrentUser>>(
      "/account/me",
      payload
    );
    return response.data.data;
  },

  // ⭐ Logout (revokes refresh token)
  logout: async (): Promise<void> => {
    try {
      await axiosInstance.post('/account/logout');
      // Clear any client-side state
      localStorage.removeItem('token');
      localStorage.removeItem('rememberMe');
      localStorage.removeItem('userEmail');
      if (typeof window !== "undefined") {
        window.location.href = '/login';
      }
    } catch (error) {
      console.error('Logout failed:', error);
      // Force logout client-side even if API call fails
      localStorage.removeItem('token');
      localStorage.removeItem('rememberMe');
      localStorage.removeItem('userEmail');
      if (typeof window !== "undefined") {
        window.location.href = '/login';
      }
    }
  },
};
