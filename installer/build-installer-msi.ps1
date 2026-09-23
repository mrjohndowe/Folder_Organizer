$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "FolderOrganizer.csproj"
$PublishDirectory = Join-Path $ProjectRoot "publish\FolderOrganizer"
$WixSource = Join-Path $PSScriptRoot "FolderOrganizer.wxs"
$OutputRoot = Join-Path $PSScriptRoot "output"

[xml]$Project =
    Get-Content $ProjectFile

$AppVersion =
    $Project.Project.PropertyGroup.Version |
    Where-Object {
        -not [string]::IsNullOrWhiteSpace($_)
    } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($AppVersion))
{
    throw "Could not read application version from $ProjectFile"
}

$AppVersion = $AppVersion.ToString().Trim()

$VersionOutputDirectory =
    Join-Path $OutputRoot $AppVersion

New-Item `
    -ItemType Directory `
    -Force `
    -Path $VersionOutputDirectory |
    Out-Null

Write-Host "Building version $AppVersion"
Write-Host "MSI output directory: $VersionOutputDirectory"

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

$WixCommand =
    Get-Command wix -ErrorAction SilentlyContinue

if (-not $WixCommand)
{
    Write-Host "WiX Toolset not found."
    Write-Host "Would you like to install it?"

    $answer = (Read-Host "Enter Yes/No").Trim().ToLowerInvariant()

    switch ($answer)
    {
        { $_ -in @("y", "yes", "Y") }
        {
            Write-Host "Installing WiX Toolset..."

            dotnet tool install --global wix

            if ($LASTEXITCODE -ne 0)
            {
                throw "WiX Toolset installation failed."
            }
        }

        { $_ -in @("n", "no", "N") }
        {
            throw "WiX Toolset is required to build the MSI."
        }

        default
        {
            throw "Invalid response. Enter y/Y/yes or n/N/no."
        }
    }
}

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
