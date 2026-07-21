#!/usr/bin/env pwsh
$DotNetInstallerUri = 'https://dot.net/v1/dotnet-install.ps1';
$DotNetUnixInstallerUri = 'https://dot.net/v1/dotnet-install.sh'
$DotNetChannel = 'LTS'
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

[string] $DotNetVersion= ''
[string] $DotNetAdditionalRuntimes = ''
foreach($line in Get-Content (Join-Path $PSScriptRoot 'build/build.config'))
{
  if ($line -like 'DOTNET_VERSION=*') {
      $DotNetVersion =$line.SubString(15)
  }
  if ($line -like 'DOTNET_ADDITIONAL_RUNTIMES=*') {
      $DotNetAdditionalRuntimes = $line.SubString(27)
  }
}

if ([string]::IsNullOrEmpty($DotNetVersion)) {
    'Failed to parse .NET Core SDK Version'
    exit 1
}

$DotNetInstallerUri = "https://dot.net/v1/dotnet-install.ps1";

# Make sure tools folder exists
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
$ToolPath = Join-Path $PSScriptRoot "tools"
if (!(Test-Path $ToolPath)) {
    Write-Verbose "Creating tools directory..."
    New-Item -Path $ToolPath -Type directory | out-null
}

###########################################################################
# INSTALL .NET CORE CLI
###########################################################################

Function Remove-PathVariable([string]$VariableToRemove)
{
  $path = [Environment]::GetEnvironmentVariable("PATH", "User")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "User")
  $path = [Environment]::GetEnvironmentVariable("PATH", "Process")
  $newItems = $path.Split(';') | Where-Object { $_.ToString() -inotlike $VariableToRemove }
  [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join(';', $newItems), "Process")
}

# Get .NET Core CLI path if installed.
$FoundDotNetCliVersion = $null;
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $FoundDotNetCliVersion = dotnet --version;
}

Write-Verbose "Checking for .net..."
if($FoundDotNetCliVersion -ne $DotNetVersion) {
    $InstallPath = Join-Path $PSScriptRoot ".dotnet"
    if (!(Test-Path $InstallPath)) {
        mkdir -Force $InstallPath | Out-Null;
    }
    Write-Verbose "Downloading .net..."
    (New-Object System.Net.WebClient).DownloadFile($DotNetInstallerUri, "$InstallPath\dotnet-install.ps1");
    & $InstallPath\dotnet-install.ps1 -Version $DotNetVersion -InstallDir $InstallPath;

    # The projects also multi-target older TFMs (e.g. net8.0), but only $DotNetVersion's
    # runtime gets installed above. Install the additional shared runtimes here so the
    # local .dotnet folder can run those TFMs too, instead of silently missing them
    # (the local dotnet.exe doesn't fall back to any machine-wide install once it's on PATH).
    if (![string]::IsNullOrEmpty($DotNetAdditionalRuntimes)) {
        foreach ($runtimeChannel in $DotNetAdditionalRuntimes.Split(',')) {
            $runtimeChannel = $runtimeChannel.Trim()
            if (![string]::IsNullOrEmpty($runtimeChannel)) {
                foreach ($runtimeType in @('dotnet', 'aspnetcore')) {
                    Write-Verbose "Downloading .NET $runtimeChannel $runtimeType runtime...";
                    & $InstallPath\dotnet-install.ps1 -Channel $runtimeChannel -Runtime $runtimeType -InstallDir $InstallPath;
                }
            }
        }
    }

    Remove-PathVariable "$InstallPath"
    $env:PATH = "$InstallPath;$env:PATH"
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
    $env:DOTNET_CLI_TELEMETRY_OPTOUT=1
}

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

dotnet run --project build/Build/Build.csproj -- $args
exit $LASTEXITCODE;