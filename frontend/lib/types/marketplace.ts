// Public user information for marketplace display
export interface PublicUserDto {
  id: string;
  userName: string;
  displayName: string;
  bio?: string;
  profilePictureUrl?: string;
  coverPhotoUrl?: string;
  joinedAt: string;
  offeredSkills: PublicSkillDto[];
  wantedSkills: PublicSkillDto[];
  completedSwaps: number;
  averageRating?: number;
  isVerified: boolean;
}

export interface PublicSkillDto {
  id: number;
  skillName: string;
  proficiencyLevel: number;
  description?: string;
  skillType: 'Offered' | 'Wanted';
}

export interface UserSearchParams {
  query?: string;
  skillName?: string;
  skillType?: 'Offered' | 'Wanted';
  category?: string;
  minProficiency?: number;
  maxProficiency?: number;
  page?: number;
  pageSize?: number;
}

export interface UserSearchResultDto {
  users: PublicUserDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SkillBrowseDto {
  skillName: string;
  userCount: number;
  averageProficiency: number;
}
