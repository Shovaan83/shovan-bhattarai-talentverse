using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentVerse.WebAPI.Data.Entities;
using TalentVerse.WebAPI.Data.Enums;

namespace TalentVerse.WebAPI.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<ProposalCounteroffer> ProposalCounteroffers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<CreditTransaction> CreditTransactions { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<GoogleCalendarToken> GoogleCalendarTokens { get; set; }
    public DbSet<VerificationRequest> VerificationRequests { get; set; }
    public DbSet<ContentReport> ContentReports { get; set; }

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

        builder.Entity<UserSkill>()
            .Property(us => us.ProficiencyLevel)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Entity<UserSkill>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_UserSkills_ProficiencyLevel",
                "\"ProficiencyLevel\" BETWEEN 1 AND 5"));

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

        builder.Entity<Proposal>()
            .Property(p => p.CreditAmount)
            .HasColumnType("decimal(18, 2)");

        builder.Entity<Proposal>()
            .Property(p => p.ProposerCreditAmount)
            .HasColumnType("decimal(18, 2)");

        builder.Entity<Proposal>()
            .Property(p => p.RecipientCreditAmount)
            .HasColumnType("decimal(18, 2)");

        builder.Entity<ProposalCounteroffer>()
            .HasOne(c => c.Proposal)
            .WithMany(p => p.Counteroffers)
            .HasForeignKey(c => c.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProposalCounteroffer>()
            .HasOne(c => c.OfferedByUser)
            .WithMany()
            .HasForeignKey(c => c.OfferedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProposalCounteroffer>()
            .Property(c => c.CreditAmount)
            .HasColumnType("decimal(18, 2)");

        builder.Entity<ProposalCounteroffer>()
            .Property(c => c.ProposerCreditAmount)
            .HasColumnType("decimal(18, 2)");

        builder.Entity<ProposalCounteroffer>()
            .Property(c => c.RecipientCreditAmount)
            .HasColumnType("decimal(18, 2)");


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

        // Appointment — creator FK (Restrict so deleting a user doesn't cascade-delete appointments)
        builder.Entity<Appointment>()
            .HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // AppointmentStatus stored as int column
        builder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<int>();

        // GoogleCalendarToken — one token record per user
        builder.Entity<GoogleCalendarToken>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GoogleCalendarToken>()
            .HasIndex(t => t.UserId)
            .IsUnique();

        // Badge — no cascade delete (badge records are permanent reference data)
        builder.Entity<UserBadge>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.UserBadges)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserBadge>()
            .HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Restrict);

        // A user can only earn each badge once
        builder.Entity<UserBadge>()
            .HasIndex(ub => new { ub.UserId, ub.BadgeId })
            .IsUnique();

        // CreditTransaction — cascade delete when user is deleted
        builder.Entity<CreditTransaction>()
            .HasOne(ct => ct.User)
            .WithMany(u => u.CreditTransactions)
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TransactionType stored as int column
        builder.Entity<CreditTransaction>()
            .Property(ct => ct.Type)
            .HasConversion<int>();

        // VerificationRequest configuration
        builder.Entity<VerificationRequest>()
            .HasOne(vr => vr.User)
            .WithMany(u => u.VerificationRequests)
            .HasForeignKey(vr => vr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VerificationRequest>()
            .HasOne(vr => vr.ReviewedBy)
            .WithMany()
            .HasForeignKey(vr => vr.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // VerificationStatus stored as int column
        builder.Entity<VerificationRequest>()
            .Property(vr => vr.Status)
            .HasConversion<int>();

        // Index for faster querying by status
        builder.Entity<VerificationRequest>()
            .HasIndex(vr => vr.Status);

        // ContentReport — reporter FK
        builder.Entity<ContentReport>()
            .HasOne(cr => cr.Reporter)
            .WithMany()
            .HasForeignKey(cr => cr.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContentReport>()
            .HasOne(cr => cr.ResolvedByAdmin)
            .WithMany()
            .HasForeignKey(cr => cr.ResolvedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ContentReport>()
            .HasIndex(cr => cr.Status);
    }
}
