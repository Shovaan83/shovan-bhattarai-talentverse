import { useQuery } from '@tanstack/react-query';
import { marketplaceApi } from '../api/marketplace';
import type { UserSearchParams } from '../types/marketplace';

export const useSearchUsers = (params: UserSearchParams) => {
  return useQuery({
    queryKey: ['marketplace', 'search', params],
    queryFn: () => marketplaceApi.searchUsers(params),
    staleTime: 30000, // 30 seconds
  });
};

export const useUserProfile = (userId: string) => {
  return useQuery({
    queryKey: ['marketplace', 'user', userId],
    queryFn: () => marketplaceApi.getUserProfile(userId),
    enabled: !!userId,
    staleTime: 60000, // 1 minute
  });
};

export const useBrowseSkills = (type?: 'Offered' | 'Wanted') => {
  return useQuery({
    queryKey: ['marketplace', 'skills', type],
    queryFn: () => marketplaceApi.browseSkills(type),
    staleTime: 120000, // 2 minutes
  });
};

export const useFeaturedUsers = () => {
  return useQuery({
    queryKey: ['marketplace', 'featured'],
    queryFn: () => marketplaceApi.getFeaturedUsers(),
    staleTime: 300000, // 5 minutes
  });
};

export const useCategories = () => {
  return useQuery({
    queryKey: ['marketplace', 'categories'],
    queryFn: () => marketplaceApi.getCategories(),
    staleTime: 600000, // 10 minutes - categories rarely change
  });
};
