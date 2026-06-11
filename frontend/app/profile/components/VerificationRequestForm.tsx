"use client";

import { useState } from "react";
import { Upload, FileText, Loader2, CheckCircle, XCircle } from "lucide-react";
import { useUploadDocument, useSubmitVerificationRequest, useVerificationStatus } from "@/lib/hooks/useVerification";
import type { VerificationStatus } from "@/lib/types/verification";

export default function VerificationRequestForm() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [documentUrl, setDocumentUrl] = useState<string>("");
  const [documentPublicId, setDocumentPublicId] = useState<string>("");

  const { data: verificationStatus, isLoading: statusLoading } = useVerificationStatus();
  const uploadMutation = useUploadDocument();
  const submitMutation = useSubmitVerificationRequest();

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type and size
      const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
      const maxSize = 10 * 1024 * 1024; // 10MB

      if (!allowedTypes.includes(file.type)) {
        alert('Please upload a PDF or image file (JPG, PNG)');
        return;
      }

      if (file.size > maxSize) {
        alert('File size must be less than 10MB');
        return;
      }

      setSelectedFile(file);
      setDocumentUrl("");
      setDocumentPublicId("");
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    try {
      const result = await uploadMutation.mutateAsync(selectedFile);
      setDocumentUrl(result.url);
      setDocumentPublicId(result.publicId);
    } catch (error) {
      console.error('Upload failed:', error);
    }
  };

  const handleSubmit = async () => {
    if (!documentUrl) return;

    await submitMutation.mutateAsync({
      documentUrl,
      documentPublicId,
    });

    // Clear form
    setSelectedFile(null);
    setDocumentUrl("");
    setDocumentPublicId("");
  };

  if (statusLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="w-6 h-6 animate-spin text-gray-400" />
      </div>
    );
  }

  const currentStatus = verificationStatus?.status || 'None';

  const renderStatusBadge = (badgeStatus: VerificationStatus) => {
    switch (badgeStatus) {
      case 'Pending':
        return (
          <div className="flex items-center gap-2 px-4 py-3 bg-yellow-50 border border-yellow-200 rounded-lg">
            <Loader2 className="w-5 h-5 text-yellow-600 animate-spin" />
            <div>
              <p className="text-sm font-medium text-yellow-900">Verification Pending</p>
              <p className="text-xs text-yellow-700">
                Your request is being reviewed by our team.
              </p>
            </div>
          </div>
        );
      case 'Approved':
        return (
          <div className="flex items-center gap-2 px-4 py-3 bg-green-50 border border-green-200 rounded-lg">
            <CheckCircle className="w-5 h-5 text-green-600" />
            <div>
              <p className="text-sm font-medium text-green-900">Verified</p>
              <p className="text-xs text-green-700">
                Your identity has been verified successfully!
              </p>
            </div>
          </div>
        );
      case 'Rejected':
        return (
          <div className="flex flex-col gap-2">
            <div className="flex items-center gap-2 px-4 py-3 bg-red-50 border border-red-200 rounded-lg">
              <XCircle className="w-5 h-5 text-red-600" />
              <div>
                <p className="text-sm font-medium text-red-900">Verification Rejected</p>
                {verificationStatus?.rejectionReason && (
                  <p className="text-xs text-red-700 mt-1">
                    Reason: {verificationStatus.rejectionReason}
                  </p>
                )}
              </div>
            </div>
            <p className="text-xs text-gray-600">
              You can submit a new verification request below.
            </p>
          </div>
        );
      default:
        return null;
    }
  };

  // If already verified, show success message
  if (currentStatus === 'Approved') {
    return renderStatusBadge('Approved');
  }

  // If pending, show status
  if (currentStatus === 'Pending') {
    return renderStatusBadge('Pending');
  }

  // If rejected or none, show upload form
  return (
    <div className="space-y-4">
      {currentStatus === 'Rejected' && renderStatusBadge('Rejected')}

      <div className="border-2 border-dashed border-gray-300 rounded-lg p-6 hover:border-gray-400 transition-colors">
        <div className="text-center space-y-4">
          <div className="mx-auto w-12 h-12 bg-blue-50 rounded-full flex items-center justify-center">
            <Upload className="w-6 h-6 text-blue-600" />
          </div>

          <div>
            <h3 className="text-lg font-semibold text-gray-900">
              Verify Your Identity
            </h3>
            <p className="text-sm text-gray-600 mt-1">
              Upload a government-issued ID or official document to verify your identity.
            </p>
            <p className="text-xs text-gray-500 mt-2">
              Accepted formats: PDF, JPG, PNG (max 10MB)
            </p>
          </div>

          {!selectedFile && !documentUrl && (
            <label className="inline-block">
              <input
                type="file"
                accept=".pdf,.jpg,.jpeg,.png"
                onChange={handleFileChange}
                className="hidden"
              />
              <span className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 cursor-pointer transition-colors">
                <FileText className="w-4 h-4" />
                Choose Document
              </span>
            </label>
          )}

          {selectedFile && !documentUrl && (
            <div className="space-y-3">
              <div className="flex items-center gap-2 justify-center text-sm text-gray-700">
                <FileText className="w-4 h-4" />
                <span className="font-medium">{selectedFile.name}</span>
                <span className="text-gray-500">
                  ({(selectedFile.size / 1024 / 1024).toFixed(2)} MB)
                </span>
              </div>
              <div className="flex gap-2 justify-center">
                <button
                  onClick={handleUpload}
                  disabled={uploadMutation.isPending}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
                >
                  {uploadMutation.isPending ? (
                    <>
                      <Loader2 className="w-4 h-4 animate-spin" />
                      Uploading...
                    </>
                  ) : (
                    <>
                      <Upload className="w-4 h-4" />
                      Upload Document
                    </>
                  )}
                </button>
                <button
                  onClick={() => setSelectedFile(null)}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}

          {documentUrl && (
            <div className="space-y-3">
              <div className="flex items-center gap-2 justify-center text-sm text-green-700">
                <CheckCircle className="w-5 h-5" />
                <span className="font-medium">Document uploaded successfully</span>
              </div>
              <button
                onClick={handleSubmit}
                disabled={submitMutation.isPending}
                className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center gap-2 mx-auto"
              >
                {submitMutation.isPending ? (
                  <>
                    <Loader2 className="w-4 h-4 animate-spin" />
                    Submitting...
                  </>
                ) : (
                  'Submit for Verification'
                )}
              </button>
            </div>
          )}
        </div>
      </div>

      <div className="text-xs text-gray-500 space-y-1">
        <p>• Your document will be reviewed by Barterly admins</p>
        <p>• Verification typically takes 1-3 business days</p>
        <p>• Verified users receive a special badge and 25 credits</p>
        <p>• Documents are securely stored and only used for verification purposes</p>
      </div>
    </div>
  );
}
