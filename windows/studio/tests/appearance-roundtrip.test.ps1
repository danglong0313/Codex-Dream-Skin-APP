$ErrorActionPreference = 'Stop'
$helper = Join-Path (Split-Path -Parent $PSScriptRoot) 'engine\scripts\appearance-config.ps1'
. $helper

function Assert-Equal {
  param([string]$Actual, [string]$Expected, [string]$Message)
  if ($Actual -ne $Expected) {
    throw "$Message`r`nExpected: $Expected`r`nActual:   $Actual"
  }
}

$original = @'
model = "gpt-test"

[desktop]
appearanceTheme = "system"
appearanceLightCodeThemeId = "solarized-light"
appearanceLightChromeTheme = { accent = "#AA55CC", surface = "#FFFFFF" }
conversationDetailMode = "STEPS_PROSE"

[features]
memories = true
'@

$snapshot = Get-DreamAppearanceSnapshot -Content $original | ConvertTo-Json -Depth 6 | ConvertFrom-Json
$applied = Set-DreamAppearanceOverrides -Content $original
if (-not (Test-DreamManagedAppearance -Content $applied)) {
  throw 'Miku appearance overrides were not applied.'
}
$restored = Restore-DreamAppearanceSnapshot -Content $applied -Snapshot $snapshot

foreach ($key in @('appearanceTheme', 'appearanceLightCodeThemeId', 'appearanceLightChromeTheme')) {
  $pattern = "(?m)^[ \t]*$([regex]::Escape($key))\s*=.*$"
  $expected = [regex]::Match($original, $pattern).Value.Trim()
  $actual = [regex]::Match($restored, $pattern).Value.Trim()
  Assert-Equal -Actual $actual -Expected $expected -Message "The original $key setting was not restored."
}

$nestedOriginal = @'
model = "gpt-test"

[desktop]
appearanceTheme = "system"
appearanceLightCodeThemeId = "raycast"

[desktop.appearanceLightChromeTheme]
accent = "#112233"
contrast = 44
ink = "#223344"
opaqueWindows = false
surface = "#FFFFFF"

[desktop.appearanceLightChromeTheme.fonts]
code = "Cascadia Mono"
ui = "Segoe UI"

[desktop.appearanceLightChromeTheme.semanticColors]
diffAdded = "#DDF5E5"
diffRemoved = "#FFE4E7"
skill = "#6677CC"

[features]
memories = true
'@

$nestedSnapshot = Get-DreamAppearanceSnapshot -Content $nestedOriginal | ConvertTo-Json -Depth 6 | ConvertFrom-Json
$nestedApplied = Set-DreamAppearanceOverrides -Content $nestedOriginal
if ($nestedApplied -match '(?m)^\[desktop\.appearanceLightChromeTheme') {
  throw 'Applying the theme must remove the structured chrome-theme table before adding the inline override.'
}
if (@([regex]::Matches($nestedApplied, '(?m)^appearanceLightChromeTheme\s*=')).Count -ne 1) {
  throw 'Applying the theme must leave exactly one chrome-theme definition.'
}
$nestedRestored = Restore-DreamAppearanceSnapshot -Content $nestedApplied -Snapshot $nestedSnapshot
foreach ($section in @(
  '[desktop.appearanceLightChromeTheme]',
  '[desktop.appearanceLightChromeTheme.fonts]',
  '[desktop.appearanceLightChromeTheme.semanticColors]'
)) {
  if (-not $nestedRestored.Contains($section)) {
    throw "The structured chrome-theme section was not restored: $section"
  }
}
if ($nestedRestored -match '(?m)^appearanceLightChromeTheme\s*=') {
  throw 'Restore must remove the Miku inline chrome-theme override before restoring structured user colors.'
}

$withoutAppearance = "model = `"gpt-test`"`r`n`r`n[desktop]`r`nconversationDetailMode = `"STEPS_PROSE`"`r`n"
$emptySnapshot = Get-DreamAppearanceSnapshot -Content $withoutAppearance | ConvertTo-Json -Depth 6 | ConvertFrom-Json
$emptyRoundTrip = Restore-DreamAppearanceSnapshot -Content (Set-DreamAppearanceOverrides -Content $withoutAppearance) -Snapshot $emptySnapshot
foreach ($key in @('appearanceTheme', 'appearanceLightCodeThemeId', 'appearanceLightChromeTheme')) {
  if ([regex]::IsMatch($emptyRoundTrip, "(?m)^[ \t]*$([regex]::Escape($key))\s*=")) {
    throw "Restore should remove $key when it did not exist before applying the theme."
  }
}

Write-Host 'PASS: appearance settings round-trip to the exact pre-apply values.'
