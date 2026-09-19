[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Add-Type -AssemblyName System.Drawing

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = Split-Path -Parent $scriptDirectory
$sourceDirectory = Join-Path $repositoryRoot 'docs\icon-options\app-icon-r2'
$resourceDirectory = Join-Path $repositoryRoot 'src\CodexUsageTrayLite\Resources'
$previewDirectory = Join-Path $repositoryRoot 'docs\icon-options\app-icon-final'
$sizes = @(16, 20, 24, 32, 40, 48, 64, 96, 128, 256)
$alphaNoiseCutoff = 4

New-Item -ItemType Directory -Path $resourceDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $previewDirectory -Force | Out-Null

function New-CleanSourceBitmap([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Icon source is missing: $path"
    }

    $loaded = [Drawing.Bitmap]::FromFile($path)
    try {
        if ($loaded.Width -ne $loaded.Height -or $loaded.Width -lt 256) {
            throw "Icon source must be square and at least 256 px: $path"
        }

        foreach ($corner in @(
            $loaded.GetPixel(0, 0),
            $loaded.GetPixel($loaded.Width - 1, 0),
            $loaded.GetPixel(0, $loaded.Height - 1),
            $loaded.GetPixel($loaded.Width - 1, $loaded.Height - 1))) {
            if ($corner.A -ne 0) {
                throw "Icon source must have transparent corners: $path"
            }
        }

        $bitmap = New-Object Drawing.Bitmap $loaded.Width, $loaded.Height, ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.DrawImageUnscaled($loaded, 0, 0)
        }
        finally {
            $graphics.Dispose()
        }
    }
    finally {
        $loaded.Dispose()
    }

    $rect = New-Object Drawing.Rectangle 0, 0, $bitmap.Width, $bitmap.Height
    $data = $bitmap.LockBits($rect, [Drawing.Imaging.ImageLockMode]::ReadWrite, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $length = [Math]::Abs($data.Stride) * $data.Height
        $bytes = New-Object byte[] $length
        [Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $length)
        for ($offset = 3; $offset -lt $length; $offset += 4) {
            if ($bytes[$offset] -lt $alphaNoiseCutoff) {
                $bytes[$offset - 3] = 0
                $bytes[$offset - 2] = 0
                $bytes[$offset - 1] = 0
                $bytes[$offset] = 0
            }
        }
        [Runtime.InteropServices.Marshal]::Copy($bytes, 0, $data.Scan0, $length)
    }
    finally {
        $bitmap.UnlockBits($data)
    }

    return $bitmap
}

function New-IconFrame([Drawing.Bitmap]$source, [int]$size) {
    $bitmap = New-Object Drawing.Bitmap $size, $size, ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([Drawing.Color]::Transparent)
        $graphics.CompositingMode = [Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.CompositingQuality = [Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::HighQuality
        $destination = New-Object Drawing.Rectangle 0, 0, $size, $size
        $sourceRect = New-Object Drawing.Rectangle 0, 0, $source.Width, $source.Height
        $graphics.DrawImage($source, $destination, $sourceRect, [Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $graphics.Dispose()
    }
    return $bitmap
}

function Convert-BitmapToPngBytes([Drawing.Bitmap]$bitmap) {
    $stream = New-Object IO.MemoryStream
    try {
        $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
        return $stream.ToArray()
    }
    finally {
        $stream.Dispose()
    }
}

function Write-PngIcon([string]$path, [byte[]]$bytes) {
    [IO.File]::WriteAllBytes($path, $bytes)
}

function Write-MultiSizeIcon([string]$path, [System.Collections.IDictionary]$frames) {
    $stream = New-Object IO.MemoryStream
    $writer = New-Object IO.BinaryWriter $stream
    try {
        $orderedSizes = @($frames.Keys | Sort-Object)
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]$orderedSizes.Count)

        $offset = 6 + (16 * $orderedSizes.Count)
        foreach ($size in $orderedSizes) {
            $bytes = [byte[]]$frames[$size]
            $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
            $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]32)
            $writer.Write([UInt32]$bytes.Length)
            $writer.Write([UInt32]$offset)
            $offset += $bytes.Length
        }
        foreach ($size in $orderedSizes) {
            $writer.Write([byte[]]$frames[$size])
        }
        $writer.Flush()
        [IO.File]::WriteAllBytes($path, $stream.ToArray())
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

function Build-Variant([string]$name, [string]$sourceName, [string]$icoName) {
    $source = New-CleanSourceBitmap (Join-Path $sourceDirectory $sourceName)
    try {
        $frames = @{}
        foreach ($size in $sizes) {
            $frame = New-IconFrame $source $size
            try {
                $bytes = Convert-BitmapToPngBytes $frame
                $frames[$size] = $bytes
                Write-PngIcon (Join-Path $previewDirectory ("{0}-{1}.png" -f $name, $size)) $bytes
            }
            finally {
                $frame.Dispose()
            }
        }
        Write-MultiSizeIcon (Join-Path $resourceDirectory $icoName) $frames
        return $frames
    }
    finally {
        $source.Dispose()
    }
}

function Write-Preview([System.Collections.IDictionary]$lightFrames, [System.Collections.IDictionary]$darkFrames) {
    $cellWidth = 146
    $cellHeight = 210
    $margin = 24
    $previewSize = 128
    $width = ($margin * 2) + ($cellWidth * $sizes.Count)
    $height = ($margin * 2) + ($cellHeight * 2) + 44
    $canvas = New-Object Drawing.Bitmap $width, $height, ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    $labelFont = New-Object Drawing.Font 'Segoe UI', 10, ([Drawing.FontStyle]::Regular), ([Drawing.GraphicsUnit]::Point)
    $rowFont = New-Object Drawing.Font 'Segoe UI', 12, ([Drawing.FontStyle]::Bold), ([Drawing.GraphicsUnit]::Point)
    try {
        $graphics.Clear([Drawing.Color]::FromArgb(245, 245, 245))
        $graphics.TextRenderingHint = [Drawing.Text.TextRenderingHint]::ClearTypeGridFit
        $graphics.DrawString('Light', $rowFont, [Drawing.Brushes]::Black, 4, $margin + 54)
        $graphics.DrawString('Dark', $rowFont, [Drawing.Brushes]::Black, 4, $margin + $cellHeight + 54)
        for ($index = 0; $index -lt $sizes.Count; $index++) {
            $size = $sizes[$index]
            $x = $margin + ($cellWidth * $index)
            foreach ($row in @(0, 1)) {
                $frames = if ($row -eq 0) { $lightFrames } else { $darkFrames }
                $y = $margin + ($cellHeight * $row)
                $background = if ($row -eq 0) { [Drawing.Color]::FromArgb(45, 45, 45) } else { [Drawing.Color]::White }
                $backgroundBrush = New-Object Drawing.SolidBrush $background
                try {
                    $graphics.FillRectangle($backgroundBrush, $x + 8, $y + 8, $previewSize, $previewSize)
                }
                finally {
                    $backgroundBrush.Dispose()
                }
                $imageStream = New-Object IO.MemoryStream (,[byte[]]$frames[$size])
                $image = [Drawing.Image]::FromStream($imageStream)
                try {
                    $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
                    $graphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
                    $graphics.DrawImage($image, (New-Object Drawing.Rectangle ($x + 8), ($y + 8), $previewSize, $previewSize))
                }
                finally {
                    $image.Dispose()
                    $imageStream.Dispose()
                }
                $label = "$size px"
                $labelSize = $graphics.MeasureString($label, $labelFont)
                $graphics.DrawString($label, $labelFont, [Drawing.Brushes]::Black, $x + (($cellWidth - $labelSize.Width) / 2), $y + 146)
            }
        }
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $canvas.Save((Join-Path $previewDirectory 'c-code-meter-icon-sizes.png'), [Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $labelFont.Dispose()
        $rowFont.Dispose()
        $graphics.Dispose()
        $canvas.Dispose()
    }
}

$lightFrames = Build-Variant 'light' 'c-code-meter-light-alpha.png' 'AppIconLight.ico'
$darkFrames = Build-Variant 'dark' 'c-code-meter-dark.png' 'AppIconDark.ico'
Write-Preview $lightFrames $darkFrames

Write-Host "Generated AppIconLight.ico and AppIconDark.ico with sizes: $($sizes -join ', ')"
Write-Host "Preview: $(Join-Path $previewDirectory 'c-code-meter-icon-sizes.png')"
