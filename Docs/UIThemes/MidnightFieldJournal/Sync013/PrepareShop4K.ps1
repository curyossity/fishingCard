$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$targetFolder = 'Assets/UIThemes/MidnightFieldJournal/Harbor/TackleShop/DesignR067'
New-Item -ItemType Directory -Force $targetFolder | Out-Null
$sourceImage = [System.Drawing.Image]::FromFile((Resolve-Path 'Temp/RealESRGAN/ShopBackgroundRestored4x.png'))
$targetImage = New-Object System.Drawing.Bitmap(3840,2160)
$graphics = [System.Drawing.Graphics]::FromImage($targetImage)
try {
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.DrawImage($sourceImage,0,0,3840,2160)
    $targetImage.Save((Join-Path (Resolve-Path $targetFolder) 'ShopBackground4K.png'),[System.Drawing.Imaging.ImageFormat]::Png)
} finally { $graphics.Dispose(); $targetImage.Dispose(); $sourceImage.Dispose() }
Copy-Item 'Docs/UIThemes/MidnightFieldJournal/Revisions/MFJ-R065/Art/EquipmentCardMaster.png' "$targetFolder/EquipmentCardMaster.png"
