param([string]$OutputPath = 'docs/images/menu-english.png')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[Windows.Forms.Application]::EnableVisualStyles()
$root = Split-Path -Parent $PSScriptRoot
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $root 'src/CodexUsageTrayLite/bin/x64/Release/net48/CodexUsageTrayLite.exe'))
$theme = $assembly.GetType('CodexUsageTrayLite.UI.ThemeService')
$modeType = $assembly.GetType('CodexUsageTrayLite.Models.AppThemeMode')
[xml]$resources = Get-Content -Encoding UTF8 (Join-Path $root 'src/CodexUsageTrayLite/Resources/Strings.resx')
$labels = @{}
foreach ($entry in $resources.root.data) { $labels[$entry.name] = [string]$entry.value }
$canvas = New-Object Drawing.Bitmap 850,750
$graphics = [Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([Drawing.Color]::FromArgb(232,235,239))
$font = New-Object Drawing.Font 'Segoe UI',12
$titleFont = New-Object Drawing.Font 'Segoe UI',14,([Drawing.FontStyle]::Bold)
$graphics.DrawString('English tray menu - illustrative sample data', $titleFont, [Drawing.Brushes]::Black, 24, 14)
$index = 0
foreach ($appearance in @('Light','Dark')) {
    $menu = New-Object Windows.Forms.ContextMenuStrip
    $menu.Font = $font
    foreach ($text in @('Email: demo@example.com','User Level: Plus','5-Hour: 71%','Weekly: 81%','Usage resets: 2','Last updated: 12:30 (WebView2)')) {
        $item = New-Object Windows.Forms.ToolStripMenuItem $text
        $item.Enabled = $false
        [void]$menu.Items.Add($item)
    }
    foreach ($key in @('-','RefreshNow','OpenLogin','-','UsageSourceMenu','RefreshInterval','IconStyleMenu','ProxySettings','ClearSession','-','StartAtLogin','Theme','LanguageMenu','-','OpenLogs','OpenConfig','Help','About','-','Exit')) {
        if ($key -eq '-') { [void]$menu.Items.Add((New-Object Windows.Forms.ToolStripSeparator)); continue }
        $item = New-Object Windows.Forms.ToolStripMenuItem $labels[$key]
        $children = switch ($key) {
            'UsageSourceMenu' { @('WebView2','Codex CLI') }
            'RefreshInterval' { @('1 minute','2 minutes','5 minutes','10 minutes','15 minutes','30 minutes','60 minutes') }
            'IconStyleMenu' { @('5-Hour Only','Weekly Only','Both (Default)','Side by Side') }
            'Theme' { @('Follow Windows','Light','Dark') }
            'LanguageMenu' { @('English','Simplified Chinese','Traditional Chinese','Japanese','Korean') }
        }
        foreach ($child in $children) { [void]$item.DropDownItems.Add($child) }
        [void]$menu.Items.Add($item)
    }
    [void]$theme.GetMethod('ApplyToMenu').Invoke($null,@($menu.PSObject.BaseObject,[Enum]::Parse($modeType,$appearance)))
    $menu.CreateControl()
    $menu.PerformLayout()
    $menu.Size = $menu.GetPreferredSize([Drawing.Size]::Empty)
    $bitmap = New-Object Drawing.Bitmap $menu.Width,$menu.Height
    $menu.DrawToBitmap($bitmap,(New-Object Drawing.Rectangle 0,0,$menu.Width,$menu.Height))
    $x = 24 + $index * 415
    $graphics.DrawString($appearance,$font,[Drawing.Brushes]::Black,$x,48)
    $graphics.DrawImageUnscaled($bitmap,$x,80)
    $bitmap.Dispose()
    $menu.Dispose()
    $index++
}
$canvas.Save((Join-Path $root $OutputPath),[Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $canvas.Dispose(); $font.Dispose(); $titleFont.Dispose()
