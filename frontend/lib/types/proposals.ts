// Proposal status enum
export type ProposalStatus = "Pending" | "Accepted" | "Rejected" | "Completed" | "Cancelled";

// Full proposal details for single proposal view
export interface Proposal {
  proposalId: number;
  creditAmount: number;
  
  // Proposer info
  proposerId: string;
  proposerUsername: string;
  proposerProfilePicture?: string;
  
  // Recipient info
  recipientId: string;
  recipientUsername: string;
  recipientProfilePicture?: string;
  
  // Skill being offered by proposer
  proposerUserSkillId: number;
  proposerSkillName: string;
  proposerSkillCategory: string;
  proposerSkillDescription?: string;
  
  // Skill being requested from recipient
  recipientUserSkillId: number;
  recipientSkillName: string;
  recipientSkillCategory: string;
  recipientSkillDescription?: string;
  
  // Status info
  status: ProposalStatus;
  proposerConfirmed: boolean;
  recipientConfirmed: boolean;
  
  // Timestamps
  createdAt: string;
  updatedAt: string;
  
  // Action flags
  canAccept: boolean;
  canDecline: boolean;
  canCancel: boolean;
  canConfirmCompletion: boolean;
  canCounteroffer: boolean;

  counteroffers: ProposalCounteroffer[];
}

// Lightweight proposal for list views
export interface ProposalListItem {
  proposalId: number;
  creditAmount: number;
  
  // Other party info
  otherUserId: string;
  otherUsername: string;
  otherProfilePicture?: string;
  
  // Skills
  offeringSkillName: string;
  receivingSkillName: string;
  
  // Status
  status: ProposalStatus;
  proposerConfirmed: boolean;
  recipientConfirmed: boolean;
  
  // Is current user the proposer?
  isProposer: boolean;
  
  // Timestamps
  createdAt: string;
  updatedAt: string;
}

// Paginated response
export interface ProposalListResponse {
  proposals: ProposalListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Filter options
export interface ProposalFilter {
  direction?: "sent" | "received";
  status?: ProposalStatus;
  searchQuery?: string;
  sortBy?: "UpdatedAt" | "CreatedAt" | "Status";
  sortOrder?: "asc" | "desc";
  dateFrom?: string; // ISO date string
  dateTo?: string; // ISO date string
  page?: number;
  pageSize?: number;
}

// Create proposal payload
export interface CreateProposalPayload {
  proposerUserSkillId: number;
  recipientUserSkillId: number;
  creditAmount: number;
  message?: string;
}

export interface CreateCounterofferPayload {
  creditAmount: number;
  message?: string;
}

export interface ProposalCounteroffer {
  proposalCounterofferId: number;
  proposalId: number;
  offeredByUserId: string;
  offeredByUsername: string;
  creditAmount: number;
  message?: string;
  createdAt: string;
}
