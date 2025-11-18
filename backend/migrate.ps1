# Migration Script for TalentVerse
# This script creates and applies Entity Framework migrations

param(
    [string]$MigrationName = ""
)

$ErrorActionPreference = "Stop"

Write-Host "=== TalentVerse Migration Tool ===" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL container is running
Write-Host "Checking PostgreSQL container..." -ForegroundColor Yellow
$containerRunning = docker ps --filter "name=talentverse-postgres-1" --format "{{.Names}}" 2>$null

if (-not $containerRunning) {
    Write-Host "PostgreSQL container is not running. Starting..." -ForegroundColor Red
    docker-compose up -d postgres
    Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
}

# Navigate to project directory
$projectPath = Join-Path $PSScriptRoot "TalentVerse.WebAPI"
Push-Location $projectPath

try {
    # If migration name provided, create new migration
    if ($MigrationName) {
        Write-Host "Creating new migration: $MigrationName" -ForegroundColor Green
        dotnet ef migrations add $MigrationName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create migration"
        }
        Write-Host "Migration created successfully!" -ForegroundColor Green
        Write-Host ""
    }

    # Generate SQL script
    Write-Host "Generating SQL migration script..." -ForegroundColor Yellow
    dotnet ef migrations script -o migration.sql
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to generate migration script"
    }

    # Check if there are any changes
    $sqlContent = Get-Content migration.sql -Raw
    if ($sqlContent.Trim().Length -lt 100) {
        Write-Host "No pending migrations to apply." -ForegroundColor Green
        Remove-Item migration.sql -Force
        return
    }

    # Apply migration
    Write-Host "Applying migration to database..." -ForegroundColor Yellow
    Get-Content migration.sql | docker exec -i talentverse-postgres-1 psql -U postgres -d talentverse_db 2>&1 | Tee-Object -Variable output
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error applying migration:" -ForegroundColor Red
        Write-Host $output
        throw "Migration failed"
    }

    # Clean up
    Remove-Item migration.sql -Force

    Write-Host ""
    Write-Host "✅ Migration applied successfully!" -ForegroundColor Green
    Write-Host ""

    # Show applied migrations
    Write-Host "Applied migrations:" -ForegroundColor Cyan
    docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"

} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
