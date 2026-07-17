[CmdletBinding()]
param(
  [ValidateSet('Debug', 'Release')]
  [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = $PSScriptRoot
$ProjectPath = Join-Path $ProjectRoot 'CodexDreamSkinStudio.csproj'
$localDotNet = Join-Path $env:LOCALAPPDATA 'CodexDreamSkinStudio\build-sdk\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $localDotNet -PathType Leaf) {
  $localDotNet
} else {
  (Get-Command dotnet -ErrorAction Stop).Source
}

& $dotnet build $ProjectPath --nologo --configuration $Configuration --property:Platform=x64
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$output = Join-Path $ProjectRoot "bin\x64\$Configuration\net8.0-windows"
Write-Host "Codex Dream Skin Studio built: $output"
