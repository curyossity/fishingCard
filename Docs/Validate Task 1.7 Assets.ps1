param([switch]$ValidateMetadata)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
public static class OverlayAssetPixels
{
    // Inspect source alpha without modifying the generated artwork.
    public static int[] Inspect(string path)
    {
        using (var b = new Bitmap(path))
        {
            int left=b.Width, top=b.Height, right=-1, bottom=-1, min=255, max=0;
            for(int y=0; y<b.Height; y++)
                for(int x=0; x<b.Width; x++)
                {
                    int a=b.GetPixel(x,y).A;
                    min=Math.Min(min,a); max=Math.Max(max,a);
                    if(a>=128) { left=Math.Min(left,x); top=Math.Min(top,y); right=Math.Max(right,x); bottom=Math.Max(bottom,y); }
                }
            return new[]{b.Width,b.Height,min,max,left,top,right,bottom,(int)b.GetPixel(b.Width/2,b.Height/2).A,(int)b.GetPixel(0,0).A};
        }
    }
}
'@
$root = Split-Path $PSScriptRoot -Parent
$record = Get-Content (Join-Path $PSScriptRoot 'Task 1.7 Image Generation.json') -Raw | ConvertFrom-Json
if ($record.assets.Count -ne 27) { throw 'Expected exactly 27 Task 1.7 assets.' }
$guids = @{}
$results = foreach ($asset in $record.assets) {
    $path = Join-Path $root ('Assets/FishingUIAssets/' + $asset.path)
    $p = [OverlayAssetPixels]::Inspect($path)
    if ($asset.kind -eq 'scrim' -and $p[2] -lt 250) { throw "Dim source must cover every pixel: $path" }
    if ($asset.kind -eq 'icon' -and ($p[0] -ne 1254 -or $p[1] -ne 1254 -or $p[9] -ne 0)) { throw "Icon canvas or padding mismatch: $path" }
    if ($asset.kind -ne 'scrim' -and ($p[2] -ne 0 -or $p[3] -lt 1)) { throw "Missing transparency or artwork: $path" }
    if ($asset.kind -eq 'frame' -and $p[8] -ne 0) { throw "Frame center is not transparent: $path" }
    if ($ValidateMetadata) {
        if (!(Test-Path ($path + '.meta'))) { throw "Missing metadata: $path" }
        $meta = Get-Content ($path + '.meta') -Raw
        if ($meta -notmatch 'spriteMode: 1' -or $meta -notmatch 'textureCompression: 0') { throw "Invalid import settings: $path" }
        if ($meta -notmatch ('guid: ' + $asset.guid) -or $guids.ContainsKey($asset.guid)) { throw "Invalid or duplicate GUID: $path" }
        $guids[$asset.guid] = $true
        $rect = $asset.rect
        $border = $asset.border
        $borderText = 'spriteBorder: {x: ' + $border[0] + ', y: ' + $border[1] + ', z: ' + $border[2] + ', w: ' + $border[3] + '}'
        if (!$meta.Contains($borderText)) { throw "Metadata borders differ from record: $path" }
        if ($asset.nativeSize[0] -ne $p[0] -or $asset.nativeSize[1] -ne $p[1]) { throw "Native dimensions differ from record: $path" }
        if ($rect.x -lt 0 -or $rect.y -lt 0 -or ($rect.x + $rect.width) -gt $p[0] -or ($rect.y + $rect.height) -gt $p[1]) { throw "Sprite rectangle out of bounds: $path" }
        if (($border[0] + $border[2]) -ge $rect.width -or ($border[1] + $border[3]) -ge $rect.height) { throw "Slice borders overlap: $path" }

        $mipmaps = if ($asset.kind -eq 'icon') { 1 } else { 0 }
        $filter = if ($asset.kind -eq 'icon') { 2 } else { 1 }
        if ($meta -notmatch "enableMipMap: $mipmaps" -or $meta -notmatch "filterMode: $filter") { throw "Filtering mismatch: $path" }
        if ((Get-FileHash $path -Algorithm SHA256).Hash -ne $asset.sha256) { throw "Source artwork was modified: $path" }
    }
    [pscustomobject]@{path=$asset.path; width=$p[0]; height=$p[1]; alphaMin=$p[2]; alphaMax=$p[3]; left=$p[4]; top=$p[5]; right=$p[6]; bottom=$p[7]; center=$p[8]; corner=$p[9]}
}
$results | ConvertTo-Json -Depth 5
