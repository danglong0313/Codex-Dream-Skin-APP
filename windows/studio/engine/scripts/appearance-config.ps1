$script:DreamAppearanceKeys = @(
  'appearanceTheme',
  'appearanceLightCodeThemeId',
  'appearanceLightChromeTheme'
)

$script:DreamAppearanceSettings = [ordered]@{
  appearanceTheme = 'appearanceTheme = "light"'
  appearanceLightCodeThemeId = 'appearanceLightCodeThemeId = "codex"'
  appearanceLightChromeTheme = 'appearanceLightChromeTheme = { accent = "#12BCCA", contrast = 66, fonts = { code = "Cascadia Code", ui = "Microsoft YaHei UI" }, ink = "#174B61", opaqueWindows = true, semanticColors = { diffAdded = "#B9EEE5", diffRemoved = "#FFD1E1", skill = "#5D91E8" }, surface = "#F5FDFF" }'
}

function Get-DreamAppearanceSnapshot {
  param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content)

  $savedSettings = [ordered]@{}
  foreach ($key in $script:DreamAppearanceKeys) {
    $pattern = "(?m)^[ \t]*$([regex]::Escape($key))\s*=.*$"
    $match = [regex]::Match($Content, $pattern)
    $savedSettings[$key] = [ordered]@{
      present = $match.Success
      line = if ($match.Success) { $match.Value.Trim() } else { '' }
    }
  }

  $chromeInline = [regex]::Match($Content, '(?m)^[ \t]*appearanceLightChromeTheme\s*=.*$')
  $chromeTablePattern = '(?ms)^[ \t]*\[desktop\.appearanceLightChromeTheme(?:\.[^\]]+)?\][ \t]*\r?\n.*?(?=^[ \t]*\[|\z)'
  $chromeBlocks = @([regex]::Matches($Content, $chromeTablePattern) | ForEach-Object { $_.Value.Trim() })
  if ($chromeInline.Success) {
    $savedSettings.appearanceLightChromeTheme = [ordered]@{
      present = $true
      format = 'inline'
      line = $chromeInline.Value.Trim()
      content = $chromeInline.Value.Trim()
    }
  } elseif ($chromeBlocks.Count -gt 0) {
    $savedSettings.appearanceLightChromeTheme = [ordered]@{
      present = $true
      format = 'table'
      line = ''
      content = ($chromeBlocks -join "`r`n`r`n")
    }
  } else {
    $savedSettings.appearanceLightChromeTheme = [ordered]@{
      present = $false
      format = 'absent'
      line = ''
      content = ''
    }
  }

  return [ordered]@{
    schemaVersion = 2
    capturedAt = (Get-Date).ToString('o')
    settings = $savedSettings
  }
}

function Remove-DreamChromeThemeDefinition {
  param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content)

  $Content = [regex]::Replace(
    $Content,
    '(?m)^[ \t]*appearanceLightChromeTheme\s*=.*(?:\r?\n)?',
    '')
  $Content = [regex]::Replace(
    $Content,
    '(?ms)^[ \t]*\[desktop\.appearanceLightChromeTheme(?:\.[^\]]+)?\][ \t]*\r?\n.*?(?=^[ \t]*\[|\z)',
    '')
  return $Content
}

function Test-DreamManagedAppearance {
  param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content)

  foreach ($key in $script:DreamAppearanceKeys) {
    $pattern = "(?m)^[ \t]*$([regex]::Escape($key))\s*=.*$"
    $match = [regex]::Match($Content, $pattern)
    if (-not $match.Success -or $match.Value.Trim() -ne $script:DreamAppearanceSettings[$key]) {
      return $false
    }
  }
  return $true
}

function Add-DreamDesktopSetting {
  param(
    [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
    [Parameter(Mandatory = $true)][string]$Line
  )

  $desktop = [regex]::Match($Content, '(?ms)^\[desktop\]\s*\r?\n(?<body>.*?)(?=^\[|\z)')
  if (-not $desktop.Success) {
    $Content = $Content.TrimEnd() + "`r`n`r`n[desktop]`r`n"
    $desktop = [regex]::Match($Content, '(?ms)^\[desktop\]\s*\r?\n(?<body>.*?)(?=^\[|\z)')
  }

  $body = $desktop.Groups['body'].Value.TrimEnd()
  if ($body) { $body += "`r`n" }
  $body += $Line.Trim() + "`r`n"
  return $Content.Substring(0, $desktop.Groups['body'].Index) + $body +
    $Content.Substring($desktop.Groups['body'].Index + $desktop.Groups['body'].Length)
}

function Set-DreamAppearanceOverrides {
  param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content)

  foreach ($key in @('appearanceTheme', 'appearanceLightCodeThemeId')) {
    $pattern = "(?m)^[ \t]*$([regex]::Escape($key))\s*=.*(?:\r?\n)?"
    $Content = [regex]::Replace($Content, $pattern, '')
    $Content = Add-DreamDesktopSetting -Content $Content -Line $script:DreamAppearanceSettings[$key]
  }
  $Content = Remove-DreamChromeThemeDefinition -Content $Content
  $Content = Add-DreamDesktopSetting -Content $Content -Line $script:DreamAppearanceSettings.appearanceLightChromeTheme
  return $Content
}

function Restore-DreamAppearanceSnapshot {
  param(
    [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Content,
    [Parameter(Mandatory = $true)]$Snapshot
  )

  foreach ($key in @('appearanceTheme', 'appearanceLightCodeThemeId')) {
    $pattern = "(?m)^[ \t]*$([regex]::Escape($key))\s*=.*(?:\r?\n)?"
    $Content = [regex]::Replace($Content, $pattern, '')
    $property = $Snapshot.settings.PSObject.Properties[$key]
    if ($property -and [bool]$property.Value.present) {
      $Content = Add-DreamDesktopSetting -Content $Content -Line ([string]$property.Value.line)
    }
  }

  $Content = Remove-DreamChromeThemeDefinition -Content $Content
  $chromeProperty = $Snapshot.settings.PSObject.Properties['appearanceLightChromeTheme']
  if ($chromeProperty -and [bool]$chromeProperty.Value.present) {
    $chromeFormat = if ($chromeProperty.Value.PSObject.Properties['format']) {
      [string]$chromeProperty.Value.format
    } else {
      'inline'
    }
    $chromeContent = if ($chromeProperty.Value.PSObject.Properties['content']) {
      [string]$chromeProperty.Value.content
    } else {
      [string]$chromeProperty.Value.line
    }
    if ($chromeFormat -eq 'table') {
      $Content = $Content.TrimEnd() + "`r`n`r`n" + $chromeContent.Trim() + "`r`n"
    } else {
      $Content = Add-DreamDesktopSetting -Content $Content -Line $chromeContent
    }
  }
  return $Content
}
