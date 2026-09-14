param([switch]$ValidateMetadata)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
public static class RigAssetPixels
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
$record = Get-Content (Join-Path $PSScriptRoot 'Task 1.6 Image Generation.json') -Raw | ConvertFrom-Json
if ($record.assets.Count -ne 23) { throw 'Expected exactly 23 Task 1.6 assets.' }
$guids = @{}
$results = foreach ($asset in $record.assets) {
    $path = Join-Path $root ('Assets/FishingUIAssets/' + $asset.path)
    $p = [RigAssetPixels]::Inspect($path)
    if ($p[2] -ne 0 -or $p[3] -lt 128) { throw "Missing transparency or artwork: $path" }
    if ($asset.path -match 'frame-9slice' -and $p[8] -ne 0) { throw "Frame center is not transparent: $path" }
    if ($ValidateMetadata) {
        if (!(Test-Path ($path + '.meta'))) { throw "Missing metadata: $path" }
        $meta = Get-Content ($path + '.meta') -Raw
        if ($meta -notmatch 'spriteMode: 2' -or $meta -notmatch 'textureCompression: 0') { throw "Invalid import settings: $path" }
        if ($meta -notmatch ('guid: ' + $asset.guid) -or $guids.ContainsKey($asset.guid)) { throw "Invalid or duplicate GUID: $path" }
        $guids[$asset.guid] = $true
        $rect = $asset.rect
        $border = $asset.border
        if ($rect.x -lt 0 -or $rect.y -lt 0 -or ($rect.x + $rect.width) -gt $p[0] -or ($rect.y + $rect.height) -gt $p[1]) { throw "Sprite rectangle out of bounds: $path" }
        if (($border[0] + $border[2]) -ge $rect.width -or ($border[1] + $border[3]) -ge $rect.height) { throw "Slice borders overlap: $path" }
        foreach ($field in @('x', 'y', 'width', 'height')) {
            if ($meta -notmatch ('(?m)^        ' + $field + ': ' + $rect.$field + '\r?$')) { throw "Metadata rectangle differs from record: $path" }
        }
        if ((Get-FileHash $path).Hash -ne (Get-FileHash $asset.source).Hash) { throw "Source artwork was modified: $path" }
    }
    [pscustomobject]@{path=$asset.path; width=$p[0]; height=$p[1]; alphaMin=$p[2]; alphaMax=$p[3]; left=$p[4]; top=$p[5]; right=$p[6]; bottom=$p[7]; center=$p[8]; corner=$p[9]}
}
$results | ConvertTo-Json -Depth 5
