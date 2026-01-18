using TalentVerse.WebAPI.DTO.Skills;

namespace TalentVerse.WebAPI.Interfaces;

public interface ISkillRepository
{
    //this will return true of the skill add is successful otherwise false
    Task<bool> AddSkillToUserAsync (string userId, AddSkillDto skillDto);

    //this will return the list of skills for a particular user
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync(string userId);

    //this will delete a particular skill for a user
    Task<bool> DeleteUserSkillAsync(string userId, int userSkillId);


}
