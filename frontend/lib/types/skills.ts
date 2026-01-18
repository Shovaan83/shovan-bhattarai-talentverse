export type SkillType = "Offer" | "Want";

export interface UserSkill {
  userSkillId: number;
  skillName: string;
  category: string;
  type: SkillType;
  description: string;
}

export interface AddSkillPayload {
  skillName: string;
  category: string;
  type: 0 | 1; // 0 = Offer, 1 = Want
  description: string;
}

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  bio: string;
  avatarUrl?: string;
  swapCredits: number;
  reputationScore: number;
  totalSwaps: number;
}
