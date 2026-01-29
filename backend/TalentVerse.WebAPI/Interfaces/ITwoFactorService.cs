namespace TalentVerse.WebAPI.Interfaces
{
    public interface ITwoFactorService
    {
        string GenerateCode();
        Task StoreCodeAsync(string userId, string code);
        Task<bool> ValidateCodeAsync(string userId, string code);
    }
}
