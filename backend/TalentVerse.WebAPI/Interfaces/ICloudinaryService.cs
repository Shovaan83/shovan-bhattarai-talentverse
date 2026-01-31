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
    /// Deletes an image from Cloudinary by public ID
    /// </summary>
    /// <param name="publicId">The Cloudinary public ID of the image</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteImageAsync(string publicId);
}
