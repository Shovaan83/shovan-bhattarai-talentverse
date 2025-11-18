# TalentVerse Database Management Scripts

This folder contains helper scripts for managing database migrations and inspecting the database.

## 🚀 Quick Start Scripts

### migrate.ps1
Creates and applies Entity Framework migrations to the database.

**Usage:**
```powershell
# Apply pending migrations
.\migrate.ps1

# Create and apply new migration
.\migrate.ps1 -MigrationName "AddUserBioField"
```

**What it does:**
1. Checks if PostgreSQL container is running
2. Creates a new migration (if name provided)
3. Generates SQL script from migrations
4. Applies the script to the database
5. Shows all applied migrations

---

### check-db.ps1
Inspects database tables and data.

**Usage:**
```powershell
# List all tables and migrations
.\check-db.ps1

# View specific table structure
.\check-db.ps1 -TableName "Skills"

# View table structure AND data
.\check-db.ps1 -TableName "Skills" -ShowData

# View table with more rows
.\check-db.ps1 -TableName "AspNetUsers" -ShowData -Limit 20
```

**What it does:**
1. Checks if PostgreSQL container is running
2. Shows table list, migrations, and database size (if no table specified)
3. Shows table structure and optionally data (if table specified)

---

## 📖 Full Documentation

See [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) for complete documentation on:
- All migration methods
- PostgreSQL queries
- Troubleshooting
- Best practices
- Common workflows

---

## 🔧 Manual Commands

If you prefer to run commands manually:

### List all tables
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\dt"
```

### View table structure
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\d+ Skills"
```

### View applied migrations
```powershell
docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

### Create migration
```powershell
cd TalentVerse.WebAPI
dotnet ef migrations add YourMigrationName
```

### Apply migration via SQL script
```powershell
cd TalentVerse.WebAPI
dotnet ef migrations script -o migration.sql
Get-Content migration.sql | docker exec -i talentverse-postgres-1 psql -U postgres -d talentverse_db
Remove-Item migration.sql
```

---

## ⚠️ Important Notes

1. PostgreSQL container must be running (`docker-compose up -d postgres`)
2. Always create migrations before changing database schema
3. Use descriptive migration names (e.g., `AddEmailVerificationToUser`)
4. Review generated SQL before applying to production

---

## 🐛 Troubleshooting

**Container not running:**
```powershell
docker-compose up -d postgres
```

**Check container status:**
```powershell
docker ps
```

**View PostgreSQL logs:**
```powershell
docker logs talentverse-postgres-1
```

**Connect to PostgreSQL shell:**
```powershell
docker exec -it talentverse-postgres-1 psql -U postgres -d talentverse_db
```
