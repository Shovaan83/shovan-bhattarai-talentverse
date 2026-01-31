export interface CurrentUser {
  username: string;
  email: string;
  bio?: string | null;
  profilePictureUrl?: string | null;
  isProfileComplete: boolean;
  location?: string | null;
  gitHubUrl?: string | null;
  linkedInUrl?: string | null;
  twitterUrl?: string | null;
  websiteUrl?: string | null;
}

export interface UpdateProfilePayload {
  username?: string;
  bio?: string | null;
  profilePictureUrl?: string | null;
}

export interface SocialLinks {
  gitHubUrl?: string;
  linkedInUrl?: string;
  twitterUrl?: string;
  websiteUrl?: string;
}

export interface CompleteOnboardingPayload {
  bio?: string;
  location: string;
  profilePictureUrl: string;
  socialLinks?: SocialLinks;
}

export interface ImageUploadResult {
  url: string;
  publicId: string;
  width: number;
  height: number;
  format: string;
}
