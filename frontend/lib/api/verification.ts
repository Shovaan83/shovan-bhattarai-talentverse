import api from '../axios';
import type {
  VerificationStatusDto,
  SubmitVerificationRequestDto,
  UploadDocumentResponse,
  AdminVerificationListDto,
  VerificationRequestDto,
  ReviewVerificationDto,
} from '../types/verification';

export const verificationApi = {
  // User endpoints
  getMyStatus: async (): Promise<VerificationStatusDto> => {
    const response = await api.get('/verification/status');
    return response.data.data;
  },

  uploadDocument: async (file: File): Promise<UploadDocumentResponse> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post('/verification/upload-document', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data.data;
  },

  submitRequest: async (dto: SubmitVerificationRequestDto): Promise<VerificationStatusDto> => {
    const response = await api.post('/verification/submit', dto);
    return response.data.data;
  },

  // Admin endpoints
  getPendingRequests: async (page = 1, pageSize = 20): Promise<AdminVerificationListDto> => {
    const response = await api.get('/admin/verifications', {
      params: { page, pageSize },
    });
    return response.data.data;
  },

  getRequestById: async (id: number): Promise<VerificationRequestDto> => {
    const response = await api.get(`/admin/verifications/${id}`);
    return response.data.data;
  },

  reviewRequest: async (id: number, dto: ReviewVerificationDto): Promise<boolean> => {
    const response = await api.post(`/admin/verifications/${id}/review`, dto);
    return response.data.data;
  },
};
