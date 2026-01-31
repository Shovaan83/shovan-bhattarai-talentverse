import axiosInstance from "@/lib/axios";
import type { UserSkill, AddSkillPayload } from "@/lib/types/skills";

// API response wrapper type matching backend ServiceResponse<T>
interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors?: string[];
}

export const skillsApi = {
  // Fetch all user skills
  getMySkills: async (): Promise<UserSkill[]> => {
    const response = await axiosInstance.get<ServiceResponse<UserSkill[]>>("/skills/my-skills");
    return response.data.data ?? [];
  },

  // Add a new skill
  addSkill: async (payload: AddSkillPayload): Promise<boolean> => {
    const response = await axiosInstance.post<ServiceResponse<boolean>>("/skills", payload);
    return response.data.data;
  },

  // Delete a skill
  deleteSkill: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/skills/${id}`);
  },
};
