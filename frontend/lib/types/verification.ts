export type VerificationStatus = 'None' | 'Pending' | 'Approved' | 'Rejected';

export interface VerificationStatusDto {
  status: VerificationStatus;
  isVerified: boolean;
  submittedAt?: string;
  reviewedAt?: string;
  rejectionReason?: string;
}

export interface VerificationRequestDto {
  id: number;
  userId: string;
  userName: string;
  userEmail: string;
  userProfilePictureUrl?: string;
  documentUrl: string;
  status: VerificationStatus;
  submittedAt: string;
  reviewedAt?: string;
  reviewedByUserName?: string;
  adminNotes?: string;
  rejectionReason?: string;
}

export interface AdminVerificationListDto {
  requests: VerificationRequestDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SubmitVerificationRequestDto {
  documentUrl: string;
  documentPublicId?: string;
}

export interface ReviewVerificationDto {
  isApproved: boolean;
  adminNotes?: string;
  rejectionReason?: string;
}

export interface UploadDocumentResponse {
  url: string;
  publicId: string;
}
