using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Account;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024; // 10MB for verification documents
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly string[] AllowedDocumentExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private static readonly string[] AllowedDocumentMimeTypes = { "image/jpeg", "image/png", "application/pdf" };

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is missing. Please set CloudName, ApiKey, and ApiSecret in appsettings.json");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Force HTTPS
    }

    public async Task<ServiceResponse<ImageUploadResultDto>> UploadImageAsync(IFormFile file)
    {
        try
        {
            // 1. Guard clause - file null check
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload attempt with no file provided");
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    AppConstant.ErrorMessages.NoImageProvided);
            }

            // 2. Validate file size (5MB limit)
            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning(
                    "Upload attempt with file size {FileSize}MB exceeding limit",
                    file.Length / 1024.0 / 1024.0);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    AppConstant.ErrorMessages.ImageTooLarge);
            }

            // 3. Validate file extension
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning(
                    "Upload attempt with invalid extension: {Extension}",
                    fileExtension);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    AppConstant.ErrorMessages.InvalidImageFormat);
            }

            // 4. Validate MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning(
                    "Upload attempt with invalid MIME type: {MimeType}",
                    file.ContentType);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    AppConstant.ErrorMessages.InvalidImageFormat);
            }

            // 5. Upload to Cloudinary with transformation
            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "talentverse/profile-pictures",
                Transformation = new Transformation()
                    .Width(400).Height(400)
                    .Crop("fill")
                    .Gravity("face") // Smart crop focusing on face if detected
                    .Quality("auto:good")
                    .FetchFormat("auto"), // Automatically deliver best format (WebP if supported)
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // 6. Verify upload success
            if (uploadResult.Error != null)
            {
                _logger.LogError(
                    "Cloudinary upload failed: {ErrorMessage}",
                    uploadResult.Error.Message);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    AppConstant.ErrorMessages.ImageUploadFailed);
            }

            // 7. Map result to DTO
            var result = new ImageUploadResultDto
            {
                Url = uploadResult.SecureUrl.ToString(),
                PublicId = uploadResult.PublicId,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Format = uploadResult.Format
            };

            _logger.LogInformation(
                "Successfully uploaded image to Cloudinary - PublicId: {PublicId}, URL: {Url}",
                result.PublicId, result.Url);

            return ServiceResponse<ImageUploadResultDto>.SuccessResponse(
                result,
                AppConstant.SuccessMessages.ProfilePictureUploaded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during image upload");
            return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                AppConstant.ErrorMessages.GenericError);
        }
    }

    public async Task<ServiceResponse<ImageUploadResultDto>> UploadVerificationDocumentAsync(IFormFile file)
    {
        try
        {
            // 1. Guard clause - file null check
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Verification document upload attempt with no file provided");
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    "No document provided.");
            }

            // 2. Validate file size (10MB limit for documents)
            if (file.Length > MaxDocumentSizeBytes)
            {
                _logger.LogWarning(
                    "Verification document upload attempt with file size {FileSize}MB exceeding limit",
                    file.Length / 1024.0 / 1024.0);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    "Document size exceeds 10MB limit.");
            }

            // 3. Validate file extension
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !AllowedDocumentExtensions.Contains(fileExtension))
            {
                _logger.LogWarning(
                    "Verification document upload attempt with invalid extension: {Extension}",
                    fileExtension);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    "Invalid document format. Allowed formats: PDF, JPG, PNG.");
            }

            // 4. Validate MIME type
            if (!AllowedDocumentMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning(
                    "Verification document upload attempt with invalid MIME type: {MimeType}",
                    file.ContentType);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    "Invalid document format. Allowed formats: PDF, JPG, PNG.");
            }

            // 5. Upload to Cloudinary
            using var stream = file.OpenReadStream();

            RawUploadResult uploadResult;

            if (fileExtension == ".pdf")
            {
                // Upload PDF as raw file (no transformation)
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "talentverse/verification-docs",
                    UniqueFilename = true,
                    Overwrite = false
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                // Upload image with basic processing
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "talentverse/verification-docs",
                    UniqueFilename = true,
                    Overwrite = false
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            // 6. Verify upload success
            if (uploadResult.Error != null)
            {
                _logger.LogError(
                    "Cloudinary verification document upload failed: {ErrorMessage}",
                    uploadResult.Error.Message);
                return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                    "Document upload failed. Please try again.");
            }

            // 7. Map result to DTO
            var result = new ImageUploadResultDto
            {
                Url = GenerateSecureDocumentUrl(uploadResult.SecureUrl.ToString(), uploadResult.PublicId),
                PublicId = uploadResult.PublicId,
                Width = 0,
                Height = 0,
                Format = fileExtension.TrimStart('.')
            };

            _logger.LogInformation(
                "Successfully uploaded verification document to Cloudinary - PublicId: {PublicId}, URL: {Url}",
                result.PublicId, result.Url);

            return ServiceResponse<ImageUploadResultDto>.SuccessResponse(
                result,
                "Document uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during verification document upload");
            return ServiceResponse<ImageUploadResultDto>.FailureResponse(
                AppConstant.ErrorMessages.GenericError);
        }
    }

    public string GenerateSecureDocumentUrl(string documentUrl, string? documentPublicId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentUrl))
            {
                return documentUrl;
            }

            var resourceType = documentUrl.Contains("/raw/", StringComparison.OrdinalIgnoreCase)
                ? "raw"
                : "image";

            var publicId = !string.IsNullOrWhiteSpace(documentPublicId)
                ? documentPublicId
                : ExtractPublicIdFromUrl(documentUrl);

            if (string.IsNullOrWhiteSpace(publicId))
            {
                return documentUrl;
            }

            var signedUrl = _cloudinary.Api.UrlImgUp
                .Secure(true)
                .Signed(true)
                .ResourceType(resourceType)
                .Type("upload")
                .BuildUrl(publicId);

            return signedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate signed document URL. Falling back to original URL.");
            return documentUrl;
        }
    }

    private static string? ExtractPublicIdFromUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (segments.Count < 5)
        {
            return null;
        }

        // Typical structure: /{cloudName}/{resourceType}/{type}/v{version}/{publicId...}
        var versionIndex = segments.FindIndex(s => s.Length > 1 && s[0] == 'v' && s.Skip(1).All(char.IsDigit));
        if (versionIndex == -1 || versionIndex + 1 >= segments.Count)
        {
            return null;
        }

        var publicIdSegments = segments.Skip(versionIndex + 1);
        return string.Join('/', publicIdSegments);
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("Delete attempt with empty publicId");
                return false;
            }

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result == "ok")
            {
                _logger.LogInformation("Successfully deleted image with PublicId: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning(
                "Failed to delete image with PublicId: {PublicId}, Result: {Result}",
                publicId, result.Result);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with PublicId: {PublicId}", publicId);
            return false;
        }
    }
}
