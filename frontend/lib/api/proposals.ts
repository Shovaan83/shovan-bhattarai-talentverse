import axiosInstance from "@/lib/axios";
import type {
  Proposal,
  ProposalListResponse,
  ProposalFilter,
  CreateProposalPayload,
  CreateCounterofferPayload,
} from "@/lib/types/proposals";

// API response wrapper type matching backend ServiceResponse<T>
interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors?: string[];
}

export const proposalsApi = {
  // Create a new proposal
  createProposal: async (payload: CreateProposalPayload): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.post<ServiceResponse<Proposal>>(
      "/proposals",
      payload
    );
    return response.data;
  },

  // Submit a counteroffer on an existing proposal
  counterofferProposal: async (
    id: number,
    payload: CreateCounterofferPayload
  ): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.post<ServiceResponse<Proposal>>(
      `/proposals/${id}/counteroffer`,
      payload
    );
    return response.data;
  },

  // Get paginated list of proposals
  getProposals: async (filter?: ProposalFilter): Promise<ProposalListResponse> => {
    const params = new URLSearchParams();
    
    if (filter?.direction) params.append("direction", filter.direction);
    if (filter?.status) params.append("status", filter.status);
    if (filter?.searchQuery) params.append("searchQuery", filter.searchQuery);
    if (filter?.sortBy) params.append("sortBy", filter.sortBy);
    if (filter?.sortOrder) params.append("sortOrder", filter.sortOrder);
    if (filter?.dateFrom) params.append("dateFrom", filter.dateFrom);
    if (filter?.dateTo) params.append("dateTo", filter.dateTo);
    if (filter?.page) params.append("page", filter.page.toString());
    if (filter?.pageSize) params.append("pageSize", filter.pageSize.toString());
    
    const response = await axiosInstance.get<ServiceResponse<ProposalListResponse>>(
      `/proposals?${params.toString()}`
    );
    return response.data.data;
  },

  // Get a single proposal by ID
  getProposal: async (id: number): Promise<Proposal> => {
    const response = await axiosInstance.get<ServiceResponse<Proposal>>(
      `/proposals/${id}`
    );
    return response.data.data;
  },

  // Accept a proposal (recipient only)
  acceptProposal: async (id: number): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.patch<ServiceResponse<Proposal>>(
      `/proposals/${id}/accept`
    );
    return response.data;
  },

  // Decline a proposal (recipient only)
  declineProposal: async (id: number): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.patch<ServiceResponse<Proposal>>(
      `/proposals/${id}/decline`
    );
    return response.data;
  },

  // Cancel a proposal (proposer only)
  cancelProposal: async (id: number): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.patch<ServiceResponse<Proposal>>(
      `/proposals/${id}/cancel`
    );
    return response.data;
  },

  // Confirm completion (both parties must confirm)
  confirmCompletion: async (id: number): Promise<ServiceResponse<Proposal>> => {
    const response = await axiosInstance.patch<ServiceResponse<Proposal>>(
      `/proposals/${id}/confirm-completion`
    );
    return response.data;
  },
};
