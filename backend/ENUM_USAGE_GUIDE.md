# Enum Usage Guide

## Overview
The database now uses integer enums instead of strings for better performance and type safety.

## Available Enums

### SkillType
**Location:** `TalentVerse.WebAPI.Data.Enums.SkillType`

```csharp
public enum SkillType
{
    Offer = 0,  // User offers this skill
    Want = 1    // User wants to learn this skill
}
```

**Usage Example:**
```csharp
var userSkill = new UserSkill
{
    UserId = "user123",
    SkillId = 5,
    Type = SkillType.Offer,  // or SkillType.Want
    Description = "5 years of experience in web development",
    CreatedAt = DateTime.UtcNow
};
```

### ProposalStatus
**Location:** `TalentVerse.WebAPI.Data.Enums.ProposalStatus`

```csharp
public enum ProposalStatus
{
    Pending = 0,     // Initial state
    Accepted = 1,    // Recipient accepted
    Rejected = 2,    // Recipient declined
    Completed = 3,   // Exchange completed
    Cancelled = 4    // Proposer cancelled
}
```

**Usage Example:**
```csharp
var proposal = new Proposal
{
    ProposerId = "user1",
    RecipientId = "user2",
    Status = ProposalStatus.Pending,  // Start as pending
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Later, update status
proposal.Status = ProposalStatus.Accepted;
proposal.UpdatedAt = DateTime.UtcNow;
```

### TransactionType
**Location:** `TalentVerse.WebAPI.Data.Enums.TransactionType`

```csharp
public enum TransactionType
{
    Credit = 0,  // Add credits to account
    Debit = 1,   // Remove credits from account
    Bonus = 2,   // Bonus credits (promotions, etc.)
    Refund = 3   // Refund credits
}
```

**Usage Example:**
```csharp
var transaction = new CreditTransaction
{
    UserId = "user123",
    Type = TransactionType.Credit,
    Amount = 100.00m,
    Description = "Completed skill exchange",
    TransactionDate = DateTime.UtcNow
};
```

## Querying with Enums

### Filter by enum value
```csharp
// Get all pending proposals
var pendingProposals = await _context.Proposals
    .Where(p => p.Status == ProposalStatus.Pending)
    .ToListAsync();

// Get all skill offers
var skillOffers = await _context.UserSkills
    .Where(us => us.Type == SkillType.Offer)
    .ToListAsync();
```

### Multiple enum values
```csharp
// Get active proposals (accepted or pending)
var activeProposals = await _context.Proposals
    .Where(p => p.Status == ProposalStatus.Pending || 
                p.Status == ProposalStatus.Accepted)
    .ToListAsync();

// Or using Contains
var activeStatuses = new[] { ProposalStatus.Pending, ProposalStatus.Accepted };
var activeProposals = await _context.Proposals
    .Where(p => activeStatuses.Contains(p.Status))
    .ToListAsync();
```

## API/DTO Mapping

When creating DTOs for API responses, you can:

### Option 1: Use enum directly
```csharp
public class ProposalDto
{
    public int ProposalId { get; set; }
    public ProposalStatus Status { get; set; }  // Will serialize as integer
}
```

### Option 2: Convert to string
```csharp
public class ProposalDto
{
    public int ProposalId { get; set; }
    public string Status { get; set; }  // "Pending", "Accepted", etc.
}

// Mapping
var dto = new ProposalDto
{
    ProposalId = proposal.ProposalId,
    Status = proposal.Status.ToString()
};
```

### Option 3: Return both
```csharp
public class ProposalDto
{
    public int ProposalId { get; set; }
    public int StatusValue { get; set; }      // 0, 1, 2, etc.
    public string StatusName { get; set; }    // "Pending", "Accepted", etc.
}
```

## Validation

```csharp
// Check if enum value is valid
if (Enum.IsDefined(typeof(ProposalStatus), statusValue))
{
    var status = (ProposalStatus)statusValue;
    // Use status
}

// In ASP.NET Core model validation
public class CreateProposalRequest
{
    [Required]
    [EnumDataType(typeof(ProposalStatus))]
    public ProposalStatus Status { get; set; }
}
```

## Migration Notes

**Old string values were automatically converted:**
- UserSkills: "Offer" → 0, "Want" → 1
- Proposals: "Pending" → 0, "Accepted" → 1, etc.
- CreditTransactions: "Credit" → 0, "Debit" → 1, etc.

**Database storage:** All enum values are stored as integers for optimal performance.
