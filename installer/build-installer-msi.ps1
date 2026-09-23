$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "FolderOrganizer.csproj"
$PublishDirectory = Join-Path $ProjectRoot "publish\FolderOrganizer"
$WixSource = Join-Path $PSScriptRoot "FolderOrganizer.wxs"
$OutputRoot = Join-Path $PSScriptRoot "output"
$VersionOutputDirectory = Join-Path $OutputRoot $AppVersion

[xml]$Project = Get-Content $ProjectFile

$AppVersion =
    $Project.Project.PropertyGroup.Version |
    Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($AppVersion))
{
    throw "Could not read application version."
}

New-Item `
    -ItemType Directory `
    -Force `
    -Path $VersionOutputDirectory |
    Out-Null

Write-Host "Publishing Folder Organizer..."

dotnet publish $ProjectFile `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $PublishDirectory

if ($LASTEXITCODE -ne 0)
{
    throw "dotnet publish failed."
}

Write-Host "Building MSI..."

wix build `
    -acceptEula wix7 `
    $WixSource `
    -arch x64 `
    -d MyAppVersion=$AppVersion `
    -d PublishDir=$PublishDirectory `
    -o "$VersionOutputDirectory\FolderOrganizer-Setup.msi"

if ($LASTEXITCODE -ne 0)
{
    throw "WiX MSI build failed."
}

Write-Host ""
Write-Host "MSI created:"
Write-Host "$VersionOutputDirectory\FolderOrganizer-Setup.msi"
