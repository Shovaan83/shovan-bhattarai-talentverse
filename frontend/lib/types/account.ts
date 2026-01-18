export interface CurrentUser {
  username: string;
  email: string;
  bio?: string | null;
  profilePictureUrl?: string | null;
}

export interface UpdateProfilePayload {
  username?: string;
  bio?: string | null;
  profilePictureUrl?: string | null;
}
