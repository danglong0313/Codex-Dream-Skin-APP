[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('Status', 'Apply', 'Reapply', 'Verify', 'Restore')]
  [string]$Action,
  [ValidateRange(1024, 65535)]
  [int]$Port = 9335,
  [string]$ResultPath
)

$ErrorActionPreference = 'Stop'
$EngineRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'Engine'
$EngineScripts = Join-Path $EngineRoot 'scripts'
$StateRoot = Join-Path $env:LOCALAPPDATA 'CodexDreamSkin'
$StatePath = Join-Path $StateRoot 'state.json'
$BackupPath = Join-Path $StateRoot 'config.before-dream-skin.toml'
$AppearanceSnapshotPath = Join-Path $StateRoot 'appearance.before-dream-skin.json'
$logLines = New-Object System.Collections.Generic.List[string]

function Test-DreamSkinCdp {
  try {
    $targets = Invoke-RestMethod -Uri "http://127.0.0.1:$Port/json/list" -TimeoutSec 1
    return [bool]($targets | Where-Object { $_.type -eq 'page' -and $_.url -like 'app://*' })
  } catch {
    return $false
  }
}

function Get-DreamSkinStatus {
  $codexRunning = [bool](Get-Process ChatGPT -ErrorAction SilentlyContinue)
  $injectorRunning = $false
  if (Test-Path -LiteralPath $StatePath -PathType Leaf) {
    try {
      $state = Get-Content -LiteralPath $StatePath -Raw | ConvertFrom-Json
      if ($state.injectorPid) {
        $injectorRunning = $null -ne (Get-Process -Id ([int]$state.injectorPid) -ErrorAction SilentlyContinue)
      }
    } catch {
      $logLines.Add("State file read failed: $($_.Exception.Message)")
    }
  }
  $cdpReady = Test-DreamSkinCdp
  $statusMessage = if ($cdpReady -and $injectorRunning) {
    'Miku Aqua 01 is active'
  } elseif ($codexRunning) {
    'Codex is running without the skin'
  } else {
    'Ready to launch'
  }
  return [ordered]@{
    success = $true
    installed = (Test-Path -LiteralPath $AppearanceSnapshotPath -PathType Leaf) -or (Test-Path -LiteralPath $BackupPath -PathType Leaf)
    codexRunning = $codexRunning
    cdpReady = $cdpReady
    injectorRunning = $injectorRunning
    skinActive = $cdpReady -and $injectorRunning
    activeTheme = 'miku-aqua'
    message = $statusMessage
    log = ($logLines -join "`r`n")
  }
}

function Invoke-StudioScript {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [hashtable]$Parameters = @{}
  )
  $script = Join-Path $EngineScripts $Name
  if (-not (Test-Path -LiteralPath $script -PathType Leaf)) {
    throw "Engine file is missing: $script"
  }
  $output = & $script @Parameters *>&1 | Out-String
  if ($output.Trim()) { $logLines.Add($output.Trim()) }
}

try {
  switch ($Action) {
    'Status' {
      $result = Get-DreamSkinStatus
    }
    'Apply' {
      Invoke-StudioScript -Name 'install-dream-skin.ps1' -Parameters @{ Port = $Port; NoShortcuts = $true }
      try {
        Invoke-StudioScript -Name 'start-dream-skin.ps1' -Parameters @{ Port = $Port; RestartExisting = $true }
      } catch {
        $launchError = $_.Exception
        try {
          Invoke-StudioScript -Name 'restore-dream-skin.ps1' -Parameters @{ Port = $Port; RestoreBaseTheme = $true }
        } catch {
          $logLines.Add("Appearance rollback failed: $($_.Exception.Message)")
        }
        throw $launchError
      }
      $result = Get-DreamSkinStatus
      $result.message = 'Miku Aqua 01 was applied and Codex was launched'
    }
    'Reapply' {
      Invoke-StudioScript -Name 'install-dream-skin.ps1' -Parameters @{ Port = $Port; NoShortcuts = $true }
      try {
        Invoke-StudioScript -Name 'start-dream-skin.ps1' -Parameters @{ Port = $Port; RestartExisting = $true }
      } catch {
        $launchError = $_.Exception
        try {
          Invoke-StudioScript -Name 'restore-dream-skin.ps1' -Parameters @{ Port = $Port; RestoreBaseTheme = $true }
        } catch {
          $logLines.Add("Appearance rollback failed: $($_.Exception.Message)")
        }
        throw $launchError
      }
      $result = Get-DreamSkinStatus
      $result.message = 'Miku Aqua 01 was reapplied'
    }
    'Verify' {
      Invoke-StudioScript -Name 'verify-dream-skin.ps1' -Parameters @{ Port = $Port }
      $result = Get-DreamSkinStatus
      $result.message = 'Dream Skin verification passed'
    }
    'Restore' {
      $restoreParameters = @{ Port = $Port; RestoreBaseTheme = $true; RestartExisting = $true }
      Invoke-StudioScript -Name 'restore-dream-skin.ps1' -Parameters $restoreParameters
      $result = Get-DreamSkinStatus
      $result.skinActive = $false
      $result.injectorRunning = $false
      $result.message = 'The skin was removed and the official appearance settings were restored'
    }
  }
} catch {
  $logLines.Add($_.Exception.ToString())
  $result = [ordered]@{
    success = $false
    installed = (Test-Path -LiteralPath $AppearanceSnapshotPath -PathType Leaf) -or (Test-Path -LiteralPath $BackupPath -PathType Leaf)
    codexRunning = [bool](Get-Process ChatGPT -ErrorAction SilentlyContinue)
    cdpReady = Test-DreamSkinCdp
    injectorRunning = $false
    skinActive = $false
    activeTheme = 'miku-aqua'
    message = $_.Exception.Message
    log = ($logLines -join "`r`n")
  }
}

$json = $result | ConvertTo-Json -Depth 6 -Compress
if ($ResultPath) {
  $resultDirectory = Split-Path -Parent $ResultPath
  if ($resultDirectory -and -not (Test-Path -LiteralPath $resultDirectory)) {
    New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null
  }
  [System.IO.File]::WriteAllText($ResultPath, $json, (New-Object System.Text.UTF8Encoding($false)))
} else {
  [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
  $json
}
