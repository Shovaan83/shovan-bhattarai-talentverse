using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Account;

namespace TalentVerse.WebAPI.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image to Cloudinary with validation and transformation
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>Service response containing image URL and metadata</returns>
    Task<ServiceResponse<ImageUploadResultDto>> UploadImageAsync(IFormFile file);

    /// <summary>
    /// Uploads a verification document (PDF or image) to Cloudinary
    /// </summary>
    /// <param name="file">The document file to upload (PDF, JPG, PNG)</param>
    /// <returns>Service response containing document URL and metadata</returns>
    Task<ServiceResponse<ImageUploadResultDto>> UploadVerificationDocumentAsync(IFormFile file);

    /// <summary>
    /// Generates a signed Cloudinary delivery URL for verification documents.
    /// Useful when account security settings block direct unsigned delivery.
    /// </summary>
    /// <param name="documentUrl">Existing stored document URL</param>
    /// <param name="documentPublicId">Optional Cloudinary public ID</param>
    /// <returns>Signed URL if it can be generated, otherwise original URL</returns>
    string GenerateSecureDocumentUrl(string documentUrl, string? documentPublicId = null);

    /// <summary>
    /// Deletes an image from Cloudinary by public ID
    /// </summary>
    /// <param name="publicId">The Cloudinary public ID of the image</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteImageAsync(string publicId);
}
