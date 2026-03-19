using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalentVerse.WebAPI.Data.Entities;

namespace TalentVerse.WebAPI.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            try
            {
                if (await userManager.Users.AnyAsync()) return;

                var roles = new List<IdentityRole>
                {
                    new IdentityRole { Name = "Member", NormalizedName = "MEMBER" },
                    new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Name = "Business", NormalizedName = "BUSINESS" }
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.Name))
                    {
                        await roleManager.CreateAsync(role);
                    }
                }

                var members = new List<AppUser>
                {
                    new AppUser
                    {
                        UserName = "shovan",
                        Email = "shovan@talentverse.com",
                        Bio = "I am Shovan, the creator of TalentVerse.",
                        ProfilePictureURL = "https://instagram.fbir1-1.fna.fbcdn.net/v/t51.2885-19/586687629_18355552537207369_677735766952371488_n.jpg?stp=dst-jpg_s150x150_tt6&efg=eyJ2ZW5jb2RlX3RhZyI6InByb2ZpbGVfcGljLmRqYW5nby4xMDgwLmMyIn0&_nc_ht=instagram.fbir1-1.fna.fbcdn.net&_nc_cat=101&_nc_oc=Q6cZ2QFMRPJC7p3xVXY1TGd36CVEGTVaPjfDMqyZ2idz-ZUv6jgJSTOjuMSCFA70BqJ-pO3Pk-LbPS-3sCyyReZZN2AR&_nc_ohc=bb8RXCg_X70Q7kNvwHFecCC&_nc_gid=q5RCd0S6jGdeNvJKUOjECA&edm=AP4sbd4BAAAA&ccb=7-5&oh=00_AfjitFTvsqPbXvLSrj9DCE-PzcDGZPpyIHF5GZMQ-_we2A&oe=692A2E55&_nc_sid=7a9f4b"
                    }
                };

                foreach (var member in members)
                {
                    var result = await userManager.CreateAsync(member, "Shovaan345@#");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(member, "Member");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            logger.LogError($"Error while creating user {member.UserName}: {error.Description}");
                        }
                    }
                }

                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@talentverse.com",
                    Bio = "I am the admin of TalentVerse."
                };

                var adminResult = await userManager.CreateAsync(admin, "adminofTalentVerse@123");

                if (adminResult.Succeeded)
                {
                    await userManager.AddToRolesAsync(admin, new[] { "Admin", "Member" });
                    logger.LogInformation("Seeding completed successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while seeding users: {ex.Message}");
                throw;
            }
        }

        public static async Task SeedBadges(AppDbContext context, ILogger logger)
        {
            try
            {
                if (await context.Badges.AnyAsync()) return;

                var badges = new List<Badge>
                {
                    new Badge
                    {
                        Name = "Welcome Aboard",
                        Description = "Joined TalentVerse and started your skill-swap journey.",
                        IconKey = "welcome",
                        Tier = "Bronze",
                        Category = "Milestone",
                        CreditReward = 0
                    },
                    new Badge
                    {
                        Name = "First Swap",
                        Description = "Completed your very first skill swap.",
                        IconKey = "first_swap",
                        Tier = "Bronze",
                        Category = "Engagement",
                        CreditReward = 5
                    },
                    new Badge
                    {
                        Name = "Swap Veteran",
                        Description = "Completed 5 skill swaps.",
                        IconKey = "swap_veteran",
                        Tier = "Silver",
                        Category = "Engagement",
                        CreditReward = 10
                    },
                    new Badge
                    {
                        Name = "Swap Master",
                        Description = "Completed 10 skill swaps.",
                        IconKey = "swap_master",
                        Tier = "Gold",
                        Category = "Engagement",
                        CreditReward = 25
                    },
                    new Badge
                    {
                        Name = "First Review",
                        Description = "Wrote your first review for a swap partner.",
                        IconKey = "first_review",
                        Tier = "Bronze",
                        Category = "Engagement",
                        CreditReward = 5
                    },
                    new Badge
                    {
                        Name = "Top Rated",
                        Description = "Maintained an average review rating of 4.5 or above.",
                        IconKey = "top_rated",
                        Tier = "Gold",
                        Category = "Skill",
                        CreditReward = 20
                    },
                    new Badge
                    {
                        Name = "Credit Saver",
                        Description = "Accumulated 100 credits.",
                        IconKey = "credit_saver",
                        Tier = "Silver",
                        Category = "Economy",
                        CreditReward = 0
                    },
                    new Badge
                    {
                        Name = "Credit Mogul",
                        Description = "Accumulated 500 credits.",
                        IconKey = "credit_mogul",
                        Tier = "Gold",
                        Category = "Economy",
                        CreditReward = 50
                    },
                    new Badge
                    {
                        Name = "Skill Sharer",
                        Description = "Listed 5 or more skills on your profile.",
                        IconKey = "skill_sharer",
                        Tier = "Bronze",
                        Category = "Skill",
                        CreditReward = 5
                    }
                };

                await context.Badges.AddRangeAsync(badges);
                await context.SaveChangesAsync();
                logger.LogInformation("Badge seeding completed. {Count} badges seeded.", badges.Count);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while seeding badges: {ex.Message}");
                throw;
            }
        }
    }
}
