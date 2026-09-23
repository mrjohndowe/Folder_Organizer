$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "FolderOrganizer.csproj"
$PublishDirectory = Join-Path $ProjectRoot "publish\FolderOrganizer"
$InstallerScript = Join-Path $PSScriptRoot "FolderOrganizer.iss"
$OutputRoot = Join-Path $PSScriptRoot "output"

Clear-Host

Write-Host ""
Write-Host "========================================"
Write-Host " Folder Organizer EXE Installer Builder"
Write-Host "========================================"
Write-Host ""

if (-not (Test-Path $ProjectFile))
{
    throw "Could not find FolderOrganizer.csproj at $ProjectFile"
}

if (-not (Test-Path $InstallerScript))
{
    throw "Could not find FolderOrganizer.iss at $InstallerScript"
}

[xml]$Project = Get-Content $ProjectFile

$AppVersion =
    $Project.Project.PropertyGroup |
    ForEach-Object { $_.Version } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Select-Object -First 1

$AppVersion = [string]$AppVersion

if ([string]::IsNullOrWhiteSpace($AppVersion))
{
    throw "Could not read <Version> from $ProjectFile"
}

$AppVersion = $AppVersion.Trim()

$VersionOutputDirectory =
    Join-Path $OutputRoot $AppVersion

New-Item `
    -ItemType Directory `
    -Force `
    -Path $VersionOutputDirectory |
    Out-Null

Write-Host "Building version $AppVersion"
Write-Host "EXE output directory: $VersionOutputDirectory"
Write-Host ""

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

Write-Host ""
Write-Host "Application published successfully."
Write-Host ""

$PossibleCompilerPaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$InnoCompiler =
    $PossibleCompilerPaths |
    Where-Object { Test-Path $_ } |
    Select-Object -First 1

if (-not $InnoCompiler)
{
    Write-Host "Inno Setup 6 was not found."
    Write-Host ""
    Write-Host "Would you like to install it?"
    Write-Host "Yes (y/Y) or No (n/N)"

    $answer =
        (Read-Host "Choice").Trim().ToLowerInvariant()

    if ($answer -in @("y", "yes"))
    {
        Write-Host ""
        Write-Host "Installing Inno Setup 6..."

        winget install `
            --id JRSoftware.InnoSetup `
            -e `
            --accept-package-agreements `
            --accept-source-agreements

        if ($LASTEXITCODE -ne 0)
        {
            throw "Inno Setup installation failed."
        }

        $InnoCompiler =
            $PossibleCompilerPaths |
            Where-Object { Test-Path $_ } |
            Select-Object -First 1

        if (-not $InnoCompiler)
        {
            throw "Inno Setup was installed, but ISCC.exe could not be found."
        }
    }
    elseif ($answer -in @("n", "no"))
    {
        throw "Inno Setup is required to build the EXE installer."
    }
    else
    {
        throw "Invalid response. Enter y/yes or n/no."
    }
}

Write-Host ""
Write-Host "Building EXE installer..."

& $InnoCompiler `
    "/DMyAppVersion=$AppVersion" `
    "/O$VersionOutputDirectory" `
    "/FFolderOrganizer-Setup" `
    $InstallerScript

if ($LASTEXITCODE -ne 0)
{
    throw "Inno Setup failed to build the installer."
}

$InstallerPath =
    Join-Path `
        $VersionOutputDirectory `
        "FolderOrganizer-Setup.exe"

if (-not (Test-Path $InstallerPath))
{
    throw "Installer build completed, but the EXE was not found at $InstallerPath"
}

Write-Host ""
Write-Host "========================================"
Write-Host " EXE Installer build complete"
Write-Host "========================================"
Write-Host ""
Write-Host "Version:"
Write-Host $AppVersion
Write-Host ""
Write-Host "Output:"
Write-Host $InstallerPath
Write-Host ""
