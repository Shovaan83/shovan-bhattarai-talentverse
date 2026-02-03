-- Migration script for existing users to set IsTwoFactorSetupComplete
-- Run this after applying the EF Core migration

-- For users with 2FA already enabled, mark setup as complete
UPDATE "AspNetUsers"
SET "IsTwoFactorSetupComplete" = TRUE
WHERE "TwoFactorEnabled" = TRUE;

-- For OAuth users (users without password), mark setup as complete (they don't need our 2FA)
UPDATE "AspNetUsers" 
SET "IsTwoFactorSetupComplete" = TRUE
WHERE "PasswordHash" IS NULL;

-- For email/password users without 2FA, mark setup as incomplete (they need to setup 2FA)
UPDATE "AspNetUsers"
SET "IsTwoFactorSetupComplete" = FALSE
WHERE "TwoFactorEnabled" = FALSE 
  AND "PasswordHash" IS NOT NULL;

-- Verify counts
SELECT 
    'Total Users' AS Category,
    COUNT(*) AS Count
FROM "AspNetUsers"

UNION ALL

SELECT 
    'Users with 2FA Enabled' AS Category,
    COUNT(*) AS Count
FROM "AspNetUsers"
WHERE "TwoFactorEnabled" = TRUE

UNION ALL

SELECT 
    'OAuth Users (No Password)' AS Category,
    COUNT(*) AS Count
FROM "AspNetUsers"
WHERE "PasswordHash" IS NULL

UNION ALL

SELECT 
    'Email/Password Users with 2FA Setup Complete' AS Category,
    COUNT(*) AS Count
FROM "AspNetUsers"
WHERE "IsTwoFactorSetupComplete" = TRUE
  AND "PasswordHash" IS NOT NULL

UNION ALL

SELECT 
    'Email/Password Users Requiring 2FA Setup' AS Category,
    COUNT(*) AS Count
FROM "AspNetUsers"
WHERE "IsTwoFactorSetupComplete" = FALSE
  AND "PasswordHash" IS NOT NULL;
