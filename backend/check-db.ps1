# Database Check Script for TalentVerse
# Quick utility to inspect database tables and data

param(
    [string]$TableName = "",
    [switch]$ShowData = $false,
    [int]$Limit = 10
)

$ErrorActionPreference = "Stop"

Write-Host "=== TalentVerse Database Inspector ===" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL container is running
$containerRunning = docker ps --filter "name=talentverse-postgres-1" --format "{{.Names}}" 2>$null

if (-not $containerRunning) {
    Write-Host "❌ PostgreSQL container is not running!" -ForegroundColor Red
    Write-Host "Start it with: docker-compose up -d postgres" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ PostgreSQL container is running" -ForegroundColor Green
Write-Host ""

if ($TableName) {
    # Show specific table
    Write-Host "Table: $TableName" -ForegroundColor Cyan
    Write-Host "Structure:" -ForegroundColor Yellow
    docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\d+ ""$TableName"""
    
    if ($ShowData) {
        Write-Host ""
        Write-Host "Data (limit $Limit):" -ForegroundColor Yellow
        docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM ""$TableName"" LIMIT $Limit;"
    }
} else {
    # Show all tables
    Write-Host "All Tables:" -ForegroundColor Cyan
    docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "\dt"
    
    Write-Host ""
    Write-Host "Applied Migrations:" -ForegroundColor Cyan
    docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT * FROM ""__EFMigrationsHistory"" ORDER BY ""MigrationId"";"
    
    Write-Host ""
    Write-Host "Database Size:" -ForegroundColor Cyan
    docker exec talentverse-postgres-1 psql -U postgres -d talentverse_db -c "SELECT pg_size_pretty(pg_database_size('talentverse_db')) as size;"
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Yellow
Write-Host "  - View specific table: .\check-db.ps1 -TableName 'Skills'" -ForegroundColor Gray
Write-Host "  - View table with data: .\check-db.ps1 -TableName 'Skills' -ShowData" -ForegroundColor Gray
Write-Host "  - Limit results: .\check-db.ps1 -TableName 'Skills' -ShowData -Limit 20" -ForegroundColor Gray
