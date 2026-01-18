using Dapper;
using System.Data;
using TalentVerse.WebAPI.Data;
using TalentVerse.WebAPI.DTO.Skills;
using TalentVerse.WebAPI.Interfaces;

namespace TalentVerse.WebAPI.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly DapperContext _context;

    public SkillRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<bool> AddSkillToUserAsync(string userId, AddSkillDto skillDto)
    {
        using var connection = _context.CreateConnection();
        connection.Open();

        // using transaction as we can insert the data into multiple tables
        using var transaction = connection.BeginTransaction();

        try
        {
            // checking of the skill exist already or not in the Skills Table
            var getSkillIdSql = "SELECT \"SkillId\" FROM \"Skills\" WHERE LOWER(\"SkillName\") = LOWER(@SkillName)";
            var skillId = await connection.QueryFirstOrDefaultAsync<int?>(getSkillIdSql, new { skillDto.SkillName }, transaction);

            // if the skill does not exist then it will create it
            if (skillId == null)
            {
                var insertSkillSql = @"
                    INSERT INTO ""Skills"" (""SkillName"", ""Category"", ""CreatedAt"", ""IsActive"")
                    VALUES (@SkillName, @Category, @CreatedAt, true)
                    RETURNING ""SkillId""";

                skillId = await connection.QuerySingleAsync<int>(insertSkillSql,
                 new { skillDto.SkillName, skillDto.Category, CreatedAt = DateTime.UtcNow }, transaction);
            }

            var inserUserSkillSql = @"
                    INSERT INTO ""UserSkills"" (""UserId"", ""SkillId"", ""Type"", ""Description"", ""CreatedAt"")
                    VALUES (@UserId, @SkillId, @Type, @Description, @CreatedAt)";

            await connection.ExecuteAsync(inserUserSkillSql, new
            {
                UserId = userId,
                SkillId = skillId,
                Type = skillDto.Type, // 0 or 1 
                Description = skillDto.Description,
                CreatedAt = DateTime.UtcNow
            }, transaction);

            transaction.Commit();
            return true;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<IEnumerable<SkillDto>> GetUserSkillsAsync(string userId)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
                SELECT us.""UserSkillId"", s.""SkillName"", s.""Category"", us.""Type"", us.""Description""
                FROM ""UserSkills"" us
                JOIN ""Skills"" s ON us.""SkillId"" = s.""SkillId""
                WHERE us.""UserId"" = @UserId";

        var result = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });

        //mapping the results and converting the int (0/1) to string (offer/want)
        return result.Select(row => new SkillDto
        {
            UserSkillId = row.UserSkillId,
            SkillName = row.SkillName,
            Category = row.Category,
            Type = row.Type == 0 ? "Offer" : "Want",
            Description = row.Description
        });
    }

    public async Task<bool> DeleteUserSkillAsync(string userId, int userSkillId)
    {
        using var connection = _context.CreateConnection();

        //Ensuring the user have that skill before deleting it
        var sql = "DELETE FROM \"UserSkills\" WHERE \"UserSkillId\" = @UserSkillId AND \"UserId\" = @UserId";

        var rowAffected = await connection.ExecuteAsync(sql, new { UserSkillId = userSkillId, UserId = userId });

        return rowAffected > 0;
    }
}
