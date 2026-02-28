# Generate Store image assets from app.ico
param(
    [string]$IcoPath = "$PSScriptRoot\..\src\WinClean\Resources\app.ico",
    [string]$OutputDir = "$PSScriptRoot\..\src\WinClean.Package\Images"
)

Add-Type -AssemblyName System.Drawing

# Extract the largest image from ICO (handles 256x256 PNG entries)
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @"
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class IcoHelper
{
    public static Bitmap LoadLargestFromIco(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        // ICO header: 2 bytes reserved, 2 bytes type, 2 bytes count
        int count = BitConverter.ToUInt16(data, 4);
        int bestIdx = -1, bestSize = 0, bestOffset = 0, bestBytes = 0;

        for (int i = 0; i < count; i++)
        {
            int entryOff = 6 + i * 16;
            int w = data[entryOff] == 0 ? 256 : data[entryOff];
            int h = data[entryOff + 1] == 0 ? 256 : data[entryOff + 1];
            int size = w * h;
            int imgBytes = BitConverter.ToInt32(data, entryOff + 8);
            int imgOffset = BitConverter.ToInt32(data, entryOff + 12);

            if (size > bestSize)
            {
                bestSize = size;
                bestIdx = i;
                bestOffset = imgOffset;
                bestBytes = imgBytes;
            }
        }

        if (bestIdx < 0) return null;

        using (var ms = new MemoryStream(data, bestOffset, bestBytes))
        {
            // Check if it's PNG (starts with 0x89 0x50)
            if (data[bestOffset] == 0x89 && data[bestOffset + 1] == 0x50)
                return new Bitmap(ms);
            else
                return (Bitmap)Image.FromStream(ms);
        }
    }

    public static void ResizeAndSave(Bitmap source, string outPath, int w, int h)
    {
        using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);

                int iconSize = Math.Min(w, h);
                int padding = (int)(iconSize * 0.08);
                int drawSize = iconSize - padding * 2;
                int x = (w - drawSize) / 2;
                int y = (h - drawSize) / 2;
                g.DrawImage(source, x, y, drawSize, drawSize);
            }
            bmp.Save(outPath, ImageFormat.Png);
        }
    }
}
"@

if (-not (Test-Path $IcoPath)) {
    Write-Error "Icon file not found: $IcoPath"
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$sourceBitmap = [IcoHelper]::LoadLargestFromIco((Resolve-Path $IcoPath).Path)
if ($null -eq $sourceBitmap) {
    Write-Error "Failed to extract image from ICO"
    exit 1
}
Write-Host "  Source icon: $($sourceBitmap.Width) x $($sourceBitmap.Height)"

$assets = @(
    @{ Name = "Square44x44Logo.png";   W = 44;  H = 44  }
    @{ Name = "SmallTile.png";         W = 71;  H = 71  }
    @{ Name = "Square150x150Logo.png"; W = 150; H = 150 }
    @{ Name = "Wide310x150Logo.png";   W = 310; H = 150 }
    @{ Name = "Square310x310Logo.png"; W = 310; H = 310 }
    @{ Name = "StoreLogo.png";         W = 50;  H = 50  }
    @{ Name = "SplashScreen.png";      W = 620; H = 300 }
)

foreach ($asset in $assets) {
    $outPath = Join-Path $OutputDir $asset.Name
    [IcoHelper]::ResizeAndSave($sourceBitmap, $outPath, $asset.W, $asset.H)
    Write-Host "  Created: $($asset.Name) ($($asset.W) x $($asset.H))"
}

$sourceBitmap.Dispose()

Write-Host ""
Write-Host "All Store assets generated in: $OutputDir"
