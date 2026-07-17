[CmdletBinding()]
param(
  [int]$Port = 9335,
  [switch]$NoShortcuts
)

$ErrorActionPreference = 'Stop'
$SkillRoot = Split-Path -Parent $PSScriptRoot
$StateRoot = Join-Path $env:LOCALAPPDATA 'CodexDreamSkin'
New-Item -ItemType Directory -Force -Path $StateRoot | Out-Null
$ConfigPath = Join-Path $HOME '.codex\config.toml'
$BackupPath = Join-Path $StateRoot 'config.before-dream-skin.toml'
$AppearanceSnapshotPath = Join-Path $StateRoot 'appearance.before-dream-skin.json'
$AppearanceHelper = Join-Path $PSScriptRoot 'appearance-config.ps1'
if (-not (Test-Path -LiteralPath $ConfigPath)) { throw "Codex config not found: $ConfigPath" }
if (-not (Test-Path -LiteralPath $AppearanceHelper)) { throw "Appearance helper not found: $AppearanceHelper" }
. $AppearanceHelper

$content = Get-Content -LiteralPath $ConfigPath -Raw
$managedAppearance = Test-DreamManagedAppearance -Content $content
if (-not (Test-Path -LiteralPath $AppearanceSnapshotPath -PathType Leaf)) {
  if ($managedAppearance -and (Test-Path -LiteralPath $BackupPath -PathType Leaf)) {
    $snapshotSource = Get-Content -LiteralPath $BackupPath -Raw
  } else {
    $snapshotSource = $content
    Copy-Item -LiteralPath $ConfigPath -Destination $BackupPath -Force
  }
  Get-DreamAppearanceSnapshot -Content $snapshotSource |
    ConvertTo-Json -Depth 6 |
    Set-Content -LiteralPath $AppearanceSnapshotPath -Encoding utf8
} elseif (-not $managedAppearance) {
  Get-DreamAppearanceSnapshot -Content $content |
    ConvertTo-Json -Depth 6 |
    Set-Content -LiteralPath $AppearanceSnapshotPath -Encoding utf8
  Copy-Item -LiteralPath $ConfigPath -Destination $BackupPath -Force
}

$content = Set-DreamAppearanceOverrides -Content $content
Set-Content -LiteralPath $ConfigPath -Value $content -Encoding utf8

if (-not $NoShortcuts) {
  $shell = New-Object -ComObject WScript.Shell
  $desktop = [Environment]::GetFolderPath('Desktop')
  $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
  $powershell = (Get-Command powershell.exe).Source
  $startScript = Join-Path $PSScriptRoot 'start-dream-skin.ps1'
  $restoreScript = Join-Path $PSScriptRoot 'restore-dream-skin.ps1'
  foreach ($folder in @($desktop, $startMenu)) {
    $shortcut = $shell.CreateShortcut((Join-Path $folder 'Codex Dream Skin.lnk'))
    $shortcut.TargetPath = $powershell
    $shortcut.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$startScript`" -Port $Port -RestartExisting"
    $shortcut.WorkingDirectory = $SkillRoot
    $shortcut.Description = 'Launch Codex with the Dream/Fiona full interface skin'
    $shortcut.Save()
  }
  $restore = $shell.CreateShortcut((Join-Path $desktop 'Codex Dream Skin - Restore.lnk'))
  $restore.TargetPath = $powershell
  $restore.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$restoreScript`" -Port $Port -RestoreBaseTheme -RestartExisting"
  $restore.WorkingDirectory = $SkillRoot
  $restore.Description = 'Remove the live Codex Dream Skin'
  $restore.Save()
}

Write-Host 'Codex Dream Skin installed. Launch it with the created shortcut or start-dream-skin.ps1.'
