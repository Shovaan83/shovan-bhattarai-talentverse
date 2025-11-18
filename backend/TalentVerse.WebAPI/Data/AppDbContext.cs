using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentVerse.WebAPI.Data.Entities;

namespace TalentVerse.WebAPI.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<CreditTransaction> CreditTransactions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserSkill>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserSkills)
            .HasForeignKey(us => us.UserId);

        builder.Entity<UserSkill>()
            .HasOne(us => us.Skill)
            .WithMany(s => s.UserSkills)
            .HasForeignKey(us => us.SkillId);

        builder.Entity<Proposal>()
            .HasOne(p => p.ProposerUserSkill)
            .WithMany()
            .HasForeignKey(p => p.ProposerUserSkillId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<Proposal>()
            .HasOne(p => p.RecipientUserSkill)
            .WithMany() 
            .HasForeignKey(p => p.RecipientUserSkillId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Entity<Proposal>()
            .HasOne(p => p.Proposer)
            .WithMany(u => u.SentProposals)
            .HasForeignKey(p => p.ProposerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Proposal>()
            .HasOne(p => p.Recipient)
            .WithMany(u => u.ReceivedProposals)
            .HasForeignKey(p => p.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Reviewee)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}