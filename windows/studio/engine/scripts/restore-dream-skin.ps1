[CmdletBinding()]
param(
  [int]$Port = 9335,
  [switch]$Uninstall,
  [switch]$RestoreBaseTheme,
  [switch]$RestartExisting
)

$ErrorActionPreference = 'Stop'
$node = (Get-Command node -ErrorAction Stop).Source
$injector = Join-Path $PSScriptRoot 'injector.mjs'
$StateRoot = Join-Path $env:LOCALAPPDATA 'CodexDreamSkin'
$StatePath = Join-Path $StateRoot 'state.json'
$BackupPath = Join-Path $StateRoot 'config.before-dream-skin.toml'
$AppearanceSnapshotPath = Join-Path $StateRoot 'appearance.before-dream-skin.json'
$AppearanceHelper = Join-Path $PSScriptRoot 'appearance-config.ps1'
$codexWasRunning = [bool](Get-Process ChatGPT -ErrorAction SilentlyContinue)

if (Test-Path -LiteralPath $StatePath) {
  try {
    $state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
    if ($state.injectorPid) { Stop-Process -Id ([int]$state.injectorPid) -Force -ErrorAction SilentlyContinue }
  } catch {}
  Remove-Item -LiteralPath $StatePath -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Milliseconds 250
try { & $node $injector --remove --port $Port --timeout-ms 3000 } catch {}

if ($Uninstall) {
  $desktop = [Environment]::GetFolderPath('Desktop')
  $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
  @(
    (Join-Path $desktop 'Codex Dream Skin.lnk'),
    (Join-Path $desktop 'Codex Dream Skin - Restore.lnk'),
    (Join-Path $startMenu 'Codex Dream Skin.lnk')
  ) | ForEach-Object { Remove-Item -LiteralPath $_ -Force -ErrorAction SilentlyContinue }
}

if ($RestoreBaseTheme) {
  $config = Join-Path $HOME '.codex\config.toml'
  if (-not (Test-Path -LiteralPath $AppearanceHelper)) { throw "Appearance helper not found: $AppearanceHelper" }
  if (-not (Test-Path -LiteralPath $config)) { throw "Codex config not found: $config" }
  . $AppearanceHelper
  $currentContent = Get-Content -LiteralPath $config -Raw

  if (Test-Path -LiteralPath $AppearanceSnapshotPath -PathType Leaf) {
    $snapshot = Get-Content -LiteralPath $AppearanceSnapshotPath -Raw | ConvertFrom-Json
  } elseif (Test-Path -LiteralPath $BackupPath -PathType Leaf) {
    $snapshot = Get-DreamAppearanceSnapshot -Content (Get-Content -LiteralPath $BackupPath -Raw) |
      ConvertTo-Json -Depth 6 |
      ConvertFrom-Json
  } elseif (Test-DreamManagedAppearance -Content $currentContent) {
    $snapshot = Get-DreamAppearanceSnapshot -Content '' | ConvertTo-Json -Depth 6 | ConvertFrom-Json
  } else {
    $snapshot = $null
  }

  if ($snapshot) {
    $currentContent = Restore-DreamAppearanceSnapshot -Content $currentContent -Snapshot $snapshot
    Set-Content -LiteralPath $config -Value $currentContent -Encoding utf8
  }
  Remove-Item -LiteralPath $AppearanceSnapshotPath -Force -ErrorAction SilentlyContinue
  Remove-Item -LiteralPath $BackupPath -Force -ErrorAction SilentlyContinue
}

if ($RestartExisting -and $codexWasRunning) {
  $mainProcesses = @(Get-Process ChatGPT -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 })
  foreach ($process in $mainProcesses) { [void]$process.CloseMainWindow() }
  Start-Sleep -Seconds 2
  Get-Process ChatGPT -ErrorAction SilentlyContinue | Stop-Process -Force
  Start-Sleep -Milliseconds 600
  $package = Get-AppxPackage OpenAI.Codex | Sort-Object Version -Descending | Select-Object -First 1
  if (-not $package) { throw 'The OpenAI.Codex Store package is not installed.' }
  $exe = Join-Path $package.InstallLocation 'app\ChatGPT.exe'
  if (-not (Test-Path -LiteralPath $exe)) { throw "Codex executable not found: $exe" }
  Start-Process -FilePath $exe
}

Write-Host 'The live Dream Skin was removed.'
