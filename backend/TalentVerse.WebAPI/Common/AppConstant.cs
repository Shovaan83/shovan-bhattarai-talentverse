namespace TalentVerse.WebAPI.Common
{
    public class AppConstant
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Member = "Member";
            public const string Business = "Business";
        }

        public static class SuccessMessages
        {
        public const string RegistrationSuccessful = "Registration successful. Welcome to TalentVerse!";
        public const string LoginSuccessful = "Login successful.";
        public const string OtpSent = "OTP has been sent to your email.";
        public const string TwofaEnabled = "2FA has been successfully enabled.";

        public const string ProposalSent = "Proposal sent successfully.";
        public const string ProposalAccepted = "Proposal accepted. Chat is now active.";
        public const string ProposalDeclined = "Proposal declined.";
        public const string ProposalCancelled = "Proposal cancelled successfully.";
        public const string ProposalCompleted = "Swap completed successfully! Both parties have confirmed.";
        public const string CompletionConfirmed = "Your completion has been confirmed. Waiting for the other party to confirm.";
        public const string ReviewSubmitted = "Review submitted successfully.";
        public const string ProfilePictureUploaded = "Profile picture uploaded successfully.";
        public const string OnboardingCompleted = "Welcome to TalentVerse! Your profile is now complete.";
        }

        public static class ErrorMessages
        {
            public const string GenericError = "An error occurred while processing your request.";
            public const string InvalidLogin = "Invalid email or password.";
            public const string UserExists = "User with this email already exists.";
            public const string InvalidOtp = "Invalid OTP code.";
            public const string OtpExpired = "OTP has expired.";

            public const string ProposalNotFound = "Proposal not found.";
            public const string InvalidStateTransition = "This action is not allowed in the current proposal state.";
            public const string SelfSwapError = "You cannot send a proposal to yourself.";
            public const string DuplicateProposalError = "You already have an active proposal for these skills.";
            public const string UnauthorizedProposalAction = "You are not authorized to perform this action on this proposal.";
            public const string AlreadyConfirmed = "You have already confirmed completion.";
            public const string ProfileNotComplete = "Please complete your profile before creating proposals. Add a profile picture, bio, and location to get started.";
            public const string InvalidImageFormat = "Invalid image format. Only JPEG, PNG, and WebP are allowed.";
            public const string ImageTooLarge = "Image size exceeds 5MB limit.";
            public const string ImageUploadFailed = "Failed to upload image. Please try again.";
            public const string NoImageProvided = "No image file provided.";
        }
    }
}
