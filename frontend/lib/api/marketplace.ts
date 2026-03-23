import api from '../axios';
import type { 
  PublicUserDto, 
  UserSearchParams, 
  UserSearchResultDto,
  SkillBrowseDto 
} from '../types/marketplace';

export const marketplaceApi = {
  // Search users by skills or name
  searchUsers: async (params: UserSearchParams): Promise<UserSearchResultDto> => {
    const response = await api.get('/marketplace/search', { params });
    return response.data.data;
  },

  // Get public profile of a specific user
  getUserProfile: async (userId: string): Promise<PublicUserDto> => {
    const response = await api.get(`/marketplace/users/${userId}`);
    return response.data.data;
  },

  // Browse available skills with user counts
  browseSkills: async (type?: 'Offered' | 'Wanted'): Promise<SkillBrowseDto[]> => {
    const response = await api.get('/marketplace/skills', { 
      params: type ? { type } : {} 
    });
    return response.data.data;
  },

  // Get featured/recommended users (for discovery)
  getFeaturedUsers: async (): Promise<PublicUserDto[]> => {
    const response = await api.get('/marketplace/featured');
    return response.data.data;
  },

  // Get all available skill categories
  getCategories: async (): Promise<string[]> => {
    const response = await api.get('/marketplace/categories');
    return response.data.data;
  }
};
