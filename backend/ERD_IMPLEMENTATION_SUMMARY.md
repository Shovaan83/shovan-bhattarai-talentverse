# Database Schema Update - ERD Implementation

## Summary
Updated all entity models and database schema to match the ERD specifications in `docs/AInitialERD.drawio.png`.

## Changes Made

### 1. Created Enum Types
- **SkillType** (`Data/Enums/SkillType.cs`)
  - Offer = 0
  - Want = 1

- **ProposalStatus** (`Data/Enums/ProposalStatus.cs`)
  - Pending = 0
  - Accepted = 1
  - Rejected = 2
  - Completed = 3
  - Cancelled = 4

- **TransactionType** (`Data/Enums/TransactionType.cs`)
  - Credit = 0
  - Debit = 1
  - Bonus = 2
  - Refund = 3

### 2. Entity Updates

#### AppUser (AspNetUsers table)
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)
- ✅ Added `UpdatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)
- ✅ Added `DeletedAt` (timestamp with time zone, nullable) - for soft deletes

#### Skill
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)
- ✅ Added `IsActive` (boolean, default: true) - for soft deletes

#### UserSkill
- ✅ Changed `Type` from `string` to `SkillType` enum (stored as integer)
- ✅ Added `Description` (text, nullable)
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)

#### Proposal
- ✅ Changed `Status` from `string` to `ProposalStatus` enum (stored as integer)
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)
- ✅ Added `UpdatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)

#### Message
- ✅ Added `IsRead` (boolean, default: false)

#### Review
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)

#### Appointment
- ✅ Added `CreatedAt` (timestamp with time zone, default: CURRENT_TIMESTAMP)

#### CreditTransaction
- ✅ Changed `TransactionId` from `int` to `long` (bigint in database)
- ✅ Changed `Type` from `string` to `TransactionType` enum (stored as integer)
- ✅ Added `Description` (varchar(255), nullable)

### 3. Migration Details

**Migration Name:** `20251118133614_UpdateEntitiesAccordingToERD`

**Special Handling:**
- Custom SQL migration logic to convert existing string enum values to integers:
  - `UserSkills.Type`: "Offer" → 0, "Want" → 1
  - `Proposals.Status`: "Pending" → 0, "Accepted" → 1, etc.
  - `CreditTransactions.Type`: "Credit" → 0, "Debit" → 1, etc.

**Applied:** ✅ Successfully applied to database on 2025-11-18

### 4. Database Verification

All tables verified with correct schema:
- ✅ All timestamp columns created with CURRENT_TIMESTAMP default
- ✅ All enum columns converted to integer type
- ✅ All nullable fields properly configured
- ✅ All foreign key relationships maintained

### 5. Files Modified

**New Files:**
- `Data/Enums/SkillType.cs`
- `Data/Enums/ProposalStatus.cs`
- `Data/Enums/TransactionType.cs`

**Modified Files:**
- `Data/Entities/AppUser.cs`
- `Data/Entities/Skill.cs`
- `Data/Entities/UserSkill.cs`
- `Data/Entities/Proposal.cs`
- `Data/Entities/Message.cs`
- `Data/Entities/Review.cs`
- `Data/Entities/Appointment.cs`
- `Data/Entities/CreditTransaction.cs`

**Migration Files:**
- `Migrations/20251118133614_UpdateEntitiesAccordingToERD.cs`
- `Migrations/20251118133614_UpdateEntitiesAccordingToERD.Designer.cs`
- `Migrations/AppDbContextModelSnapshot.cs` (updated)

### 6. Build Status
✅ Project builds successfully with no errors
⚠️ Only nullable reference type warnings (expected)

## Testing Recommendations

1. Test user registration with new timestamp fields
2. Test skill creation with IsActive flag
3. Test proposal status changes with enum values
4. Test credit transactions with new bigint ID and enum type
5. Test soft deletes with DeletedAt on users and IsActive on skills
6. Verify all existing data migrated correctly (string→int conversions)

## Notes

- All timestamps use UTC (timestamp with time zone)
- Enum values are stored as integers for better performance
- Soft delete columns added for future implementation of soft delete functionality
- Default values ensure all new records have proper timestamps
- Migration includes backward compatibility (Down method) to revert changes if needed
