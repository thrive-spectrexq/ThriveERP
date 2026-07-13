Write-Host "Publishing ThriveERP for Windows (x64) as a Self-Contained Single File..." -ForegroundColor Cyan

$projectPath = "src/ThriveERP.Desktop"
$outputDir = "publish/windows"

# Create a clean publish directory
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}

# Run the publish command
dotnet publish $projectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $outputDir

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=======================================================" -ForegroundColor Green
    Write-Host "Publish successful!" -ForegroundColor Green
    Write-Host "The compiled .exe and configuration files are located in:" -ForegroundColor Green
    Write-Host (Resolve-Path $outputDir).Path -ForegroundColor Yellow
    Write-Host "=======================================================" -ForegroundColor Green
    Write-Host "To deploy: Copy the contents of the '$outputDir' folder to the target business computer."
} else {
    Write-Host "Publish failed. Please check the errors above." -ForegroundColor Red
}
