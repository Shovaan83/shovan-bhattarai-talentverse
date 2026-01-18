using TalentVerse.WebAPI.Common;
using TalentVerse.WebAPI.DTO.Skills;

namespace TalentVerse.WebAPI.Interfaces
{
    public interface ISkillService
    {
        Task<ServiceResponse<bool>> AddSkillAsync(string userId, AddSkillDto addSkillDto);
        Task<ServiceResponse<IEnumerable<SkillDto>>> GetUserSkillsAsync(string userId);
        Task<ServiceResponse<bool>> DeleteSkillAsync(string userId, int userSKillId);
    }
}
