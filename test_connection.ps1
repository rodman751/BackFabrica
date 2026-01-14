# Script para probar conexión a Azure SQL Server

$serverName = "mainserverfrabrica.database.windows.net"
$database = "Productos"  # Cambia esto por la base que quieras probar
$username = "Fabrica"
$password = "Sisepuede123*"

Write-Host "🔍 Probando conexión a Azure SQL Server..." -ForegroundColor Cyan
Write-Host "Servidor: $serverName" -ForegroundColor Gray
Write-Host "Base de datos: $database" -ForegroundColor Gray
Write-Host ""

try {
    $connectionString = "Server=$serverName;Database=$database;User Id=$username;Password=$password;Encrypt=True;TrustServerCertificate=True;Connection Timeout=60;"

    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString

    Write-Host "⏳ Conectando..." -ForegroundColor Yellow
    $connection.Open()

    Write-Host "✅ ¡Conexión exitosa!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Estado: $($connection.State)" -ForegroundColor Gray
    Write-Host "Versión del servidor: $($connection.ServerVersion)" -ForegroundColor Gray

    $connection.Close()
}
catch {
    Write-Host "❌ Error de conexión:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Posibles causas:" -ForegroundColor Yellow
    Write-Host "1. Tu IP no está en el firewall de Azure SQL" -ForegroundColor Gray
    Write-Host "2. El usuario/contraseña son incorrectos" -ForegroundColor Gray
    Write-Host "3. La base de datos no existe" -ForegroundColor Gray
    Write-Host "4. Problemas de conexión a internet" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Presiona Enter para salir..."
Read-Host
