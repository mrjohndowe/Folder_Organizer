$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "FolderOrganizer.csproj"
$PublishDirectory = Join-Path $ProjectRoot "publish\FolderOrganizer"
$InstallerScript = Join-Path $PSScriptRoot "FolderOrganizer.iss"

Write-Host ""
Write-Host "========================================"
Write-Host " Folder Organizer Installer Builder"
Write-Host "========================================"
Write-Host ""

if (-not (Test-Path $ProjectFile)) {
    throw "Could not find FolderOrganizer.csproj at $ProjectFile"
}

[xml]$Project = Get-Content $ProjectFile
$AppVersion = $Project.Project.PropertyGroup.Version |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($AppVersion)) {
    throw "Could not read the application version from $ProjectFile"
}

Write-Host "Publishing Folder Organizer..."

dotnet publish $ProjectFile `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $PublishDirectory

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Write-Host ""
Write-Host "Application published successfully."
Write-Host ""

$PossibleCompilerPaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$InnoCompiler = $PossibleCompilerPaths |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $InnoCompiler) {
    Write-Host "Inno Setup 6 was not found."
    Write-Host ""
    Write-Host "Install it with:"
    Write-Host "winget install --id JRSoftware.InnoSetup -e"
    exit 1
}

Write-Host "Building installer..."

& $InnoCompiler "/DMyAppVersion=$AppVersion" $InstallerScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed to build the installer."
}

Write-Host ""
Write-Host "========================================"
Write-Host " Installer build complete"
Write-Host "========================================"
Write-Host ""
Write-Host "Output:"
Write-Host "$PSScriptRoot\output"
Write-Host ""
