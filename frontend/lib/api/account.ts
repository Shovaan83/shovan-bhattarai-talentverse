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
      "/api/account/me"
    );
    return response.data.data;
  },

  updateMe: async (payload: UpdateProfilePayload): Promise<CurrentUser> => {
    const response = await axiosInstance.put<ServiceResponse<CurrentUser>>(
      "/api/account/me",
      payload
    );
    return response.data.data;
  },
};
