Add-Type -AssemblyName System.Drawing

$icoPath = "d:\WinClean\src\WinClean\Resources\app.ico"
$outDir = "d:\WinClean\src\WinClean.Package\Images"

$icoData = [System.IO.File]::ReadAllBytes($icoPath)
$count = [BitConverter]::ToUInt16($icoData, 4)
$bestSize = 0
$bestOff = 0
$bestLen = 0
for ($i = 0; $i -lt $count; $i++) {
    $e = 6 + $i * 16
    $w = if ($icoData[$e] -eq 0) { 256 } else { $icoData[$e] }
    $h = if ($icoData[$e+1] -eq 0) { 256 } else { $icoData[$e+1] }
    if ($w * $h -gt $bestSize) {
        $bestSize = $w * $h
        $bestOff = [BitConverter]::ToInt32($icoData, $e + 12)
        $bestLen = [BitConverter]::ToInt32($icoData, $e + 8)
    }
}
$ms = New-Object System.IO.MemoryStream($icoData, $bestOff, $bestLen)
$iconBmp = New-Object System.Drawing.Bitmap($ms)
Write-Host "Loaded icon: $($iconBmp.Width)x$($iconBmp.Height)"

function Create-StoreImage {
    param([int]$W, [int]$H, [string]$OutFile, [string]$Title, [string]$Sub)

    $bmp = New-Object System.Drawing.Bitmap($W, $H, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

    $c1 = [System.Drawing.Color]::FromArgb(255, 33, 150, 243)
    $c2 = [System.Drawing.Color]::FromArgb(255, 16, 75, 122)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point(0, 0)),
        (New-Object System.Drawing.Point($W, $H)),
        $c1, $c2)
    $g.FillRectangle($brush, 0, 0, $W, $H)

    $iconSize = [Math]::Min($W, $H) * 40 / 100
    $ix = ($W - $iconSize) / 2
    $iy = $H * 22 / 100
    $g.DrawImage($iconBmp, [int]$ix, [int]$iy, [int]$iconSize, [int]$iconSize)

    $fontSize = [Math]::Min($W, $H) / 14.0
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $ty = $iy + $iconSize + $H * 4 / 100
    $g.DrawString($Title, $font, [System.Drawing.Brushes]::White, [float]($W / 2), [float]$ty, $sf)

    if ($Sub) {
        $subSize = [Math]::Min($W, $H) / 28.0
        $sfont = New-Object System.Drawing.Font("Segoe UI", $subSize, [System.Drawing.FontStyle]::Regular)
        $sBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(180, 255, 255, 255))
        $sy = $ty + $font.GetHeight($g) + $H * 2 / 100
        $g.DrawString($Sub, $sfont, $sBrush, [float]($W / 2), [float]$sy, $sf)
        $sfont.Dispose()
        $sBrush.Dispose()
    }

    $bmp.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)
    $font.Dispose()
    $sf.Dispose()
    $brush.Dispose()
    $g.Dispose()
    $bmp.Dispose()
    Write-Host "Created: $OutFile ($W x $H)"
}

Create-StoreImage -W 1080 -H 1080 -OutFile "$outDir\StoreLogo1080.png" -Title "WinClean" -Sub "Clear Clutter, Claim Space"
Create-StoreImage -W 720 -H 1080 -OutFile "$outDir\StorePoster720x1080.png" -Title "WinClean" -Sub "Clear Clutter, Claim Space"

$iconBmp.Dispose()
$ms.Dispose()
Write-Host "Done!"
