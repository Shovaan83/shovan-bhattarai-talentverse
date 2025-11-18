# TalentVerse Database Migration Guide

## 📋 Quick Reference

### Check Current Database Tables

**Option 1: List all tables**
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\dt"
```

**Option 2: View specific table structure**
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\d+ Skills"
```

**Option 3: View all data in a table**
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM \"Skills\";"
```

**Option 4: Check applied migrations**
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

---

## 🔄 Creating and Applying New Migrations

### Method 1: Generate SQL Script (RECOMMENDED - Works reliably)

**Step 1: Create a new migration**
```powershell
cd backend/TalentVerse.WebAPI
dotnet ef migrations add YourMigrationName
```

**Step 2: Generate SQL script**
```powershell
dotnet ef migrations script -o migration.sql
```

**Step 3: Apply to database**
```powershell
Get-Content migration.sql | docker exec -i talentverse-postgres-1 psql -U postgres -d talentverse_db
```

**Step 4: Clean up**
```powershell
Remove-Item migration.sql
```

---

### Method 2: Direct Update (May have connection issues)

```powershell
cd backend/TalentVerse.WebAPI
dotnet ef database update
```

**Note**: This may fail due to Windows-Docker networking issues. If it fails, use Method 1.

---

## 🚀 Automated Scripts

I've created helper scripts for you:

### Run migrations:
```powershell
.\backend\migrate.ps1
```

### Check database tables:
```powershell
.\backend\check-db.ps1
```

---

## 🔧 Common EF Core Commands

### List all migrations
```powershell
cd backend/TalentVerse.WebAPI
dotnet ef migrations list
```

### Remove last migration (if not applied)
```powershell
cd backend/TalentVerse.WebAPI
dotnet ef migrations remove
```

### Generate migration script for specific range
```powershell
# From migration1 to migration2
dotnet ef migrations script FromMigration ToMigration -o migration.sql
```

---

## 📊 Useful PostgreSQL Queries

### View table relationships
```sql
SELECT
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY';
```

### Count rows in all tables
```sql
SELECT schemaname, tablename, 
       (SELECT COUNT(*) FROM tablename) as row_count
FROM pg_tables 
WHERE schemaname = 'public';
```

---

## ⚠️ Important Notes

1. **Always create a migration before modifying database schema**
2. **Never manually edit migration files after creation**
3. **Test migrations on development database first**
4. **Keep migrations small and focused on single changes**
5. **Name migrations descriptively** (e.g., `AddProfilePictureToUser`, `CreatePaymentTable`)

---

## 🐛 Troubleshooting

### Issue: "Password authentication failed"
**Solution**: Use Method 1 (SQL Script) instead of direct database update

### Issue: "Migration already applied"
**Solution**: Check `__EFMigrationsHistory` table to see what's applied
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

### Issue: Container not running
**Solution**: Start PostgreSQL container
```powershell
docker-compose up -d postgres
```

---

## 📝 Best Practices

1. **Descriptive Names**: Use clear migration names
   ```
   ✅ AddEmailVerificationToUser
   ❌ UpdateUser
   ```

2. **One Change Per Migration**: Keep migrations focused
   ```
   ✅ AddIndexToSkillName
   ❌ UpdateMultipleTables
   ```

3. **Check Before Applying**: Always review the generated SQL
   ```powershell
   Get-Content migration.sql | Select-Object -First 50
   ```

4. **Backup Important Data**: Before major changes
   ```powershell
   docker exec talentverse-postgres-1 pg_dump -U postgres talentverse_db > backup.sql
   ```

---

## 🎯 Quick Workflow Example

```powershell
# 1. Make changes to your entity models
# 2. Create migration
cd backend/TalentVerse.WebAPI
dotnet ef migrations add AddUserBio

# 3. Generate and apply
dotnet ef migrations script -o migration.sql
Get-Content migration.sql | docker exec -i talentverse-postgres-1 psql -U postgres -d talentverse_db
Remove-Item migration.sql

# 4. Verify
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\d+ AspNetUsers"
```
