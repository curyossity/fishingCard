param()

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$assetRoot = Join-Path $root 'Assets/FishingUIAssets'
$manifestPath = Join-Path $assetRoot 'Reference/asset-manifest.md'
$runtimeFolders = @('Backgrounds', 'Cards', 'Controls', 'Effects', 'Frames', 'Icons', 'Markers', 'Meters', 'Overlays', 'Rig')
$runtime = foreach ($folder in $runtimeFolders) {
    Get-ChildItem -LiteralPath (Join-Path $assetRoot $folder) -Recurse -Filter '*.png'
}
if ($runtime.Count -ne 88) { throw "Expected 88 runtime PNGs, found $($runtime.Count)." }

$manifest = Get-Content -LiteralPath $manifestPath -Raw
$guids = @{}
$opaqueRgb = @(
    'Backgrounds/gameplay-tabletop-16x9.png',
    'Backgrounds/ivory-paper-tile.png',
    'Backgrounds/teal-paper-tile.png',
    'Frames/aged-brass-tile.png',
    'Frames/oxidized-teal-tile.png',
    'Overlays/modal-dim.png'
)
$tiles = @(
    'Backgrounds/ivory-paper-tile.png',
    'Backgrounds/teal-paper-tile.png',
    'Frames/aged-brass-tile.png',
    'Frames/oxidized-teal-tile.png'
)

foreach ($file in $runtime) {
    $relative = $file.FullName.Substring($assetRoot.Length + 1).Replace('\', '/')
    $metaPath = $file.FullName + '.meta'
    if (!(Test-Path -LiteralPath $metaPath)) { throw "Missing metadata: $relative" }
    $manifestPathEntry = ([char]96) + $relative + ([char]96)
    $manifestNameEntry = ([char]96) + $file.Name + ([char]96)
    if (!$manifest.Contains($manifestPathEntry) -and !$manifest.Contains($manifestNameEntry)) { throw "Missing manifest entry: $relative" }
    $meta = Get-Content -LiteralPath $metaPath -Raw
    $guid = [regex]::Match($meta, '(?m)^guid: ([0-9a-f]{32})$').Groups[1].Value
    if (!$guid -or $guids.ContainsKey($guid)) { throw "Missing or duplicate GUID: $relative" }
    $guids[$guid] = $relative
    if ($meta -notmatch 'textureCompression: 0' -or $meta -notmatch 'nPOTScale: 0') {
        throw "Lossless/native-size import contract failed: $relative"
    }
    if ($tiles -contains $relative) {
        if ($meta -notmatch 'wrapU: 0' -or $meta -notmatch 'wrapV: 0') { throw "Tile is not set to Repeat: $relative" }
    }
    $stream = [IO.File]::OpenRead($file.FullName)
    try {
        $header = New-Object byte[] 26
        [void]$stream.Read($header, 0, 26)
    } finally {
        $stream.Dispose()
    }
    $expectedColorType = if ($opaqueRgb -contains $relative) { 2 } else { 6 }
    if ($header[25] -ne $expectedColorType) { throw "Unexpected PNG color type for ${relative}: $($header[25])" }
}

$referenceSizes = @{
    'component-dimensions.png' = @(1920, 1080)
    'gameplay-layout-1920x1080.png' = @(1920, 1080)
    'gameplay-layout-blank-1920x1080.png' = @(1920, 1080)
    'theme-palette.png' = @(1672, 941)
    'typography-reference.png' = @(1920, 1080)
    'ui-component-contact-sheet.png' = @(2048, 2250)
    'visual-states.png' = @(1600, 1000)
}
foreach ($name in $referenceSizes.Keys) {
    $path = Join-Path $assetRoot ('Reference/' + $name)
    $metaPath = $path + '.meta'
    if (!(Test-Path -LiteralPath $path) -or !(Test-Path -LiteralPath $metaPath)) { throw "Missing reference asset: $name" }
    $bytes = [IO.File]::ReadAllBytes($path)
    $width = [Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes, 16))
    $height = [Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes, 20))
    if ($width -ne $referenceSizes[$name][0] -or $height -ne $referenceSizes[$name][1] -or $bytes[25] -ne 2) {
        throw "Reference format mismatch: $name"
    }
    $meta = Get-Content -LiteralPath $metaPath -Raw
    if ($meta -notmatch 'textureType: 0' -or $meta -notmatch 'spriteMode: 0' -or
        $meta -notmatch 'maxTextureSize: 4096' -or $meta -notmatch 'textureCompression: 0') {
        throw "Reference import settings mismatch: $name"
    }
}

foreach ($font in @(
    'Fonts/Marcellus/Marcellus-Regular.ttf',
    'Fonts/Marcellus/OFL.txt',
    'Fonts/SourceSerif4/SourceSerif4-Variable.ttf',
    'Fonts/SourceSerif4/OFL.txt',
    'Fonts/ThirdPartyNotices.txt'
)) {
    if (!(Test-Path -LiteralPath (Join-Path $assetRoot $font))) { throw "Missing font or license file: $font" }
}
if ([regex]::IsMatch($manifest, '(?m)^\|.*\bTBD\b.*\|$')) { throw 'Manifest still contains a TBD data row.' }

[pscustomobject]@{
    runtimePngs = $runtime.Count
    referencePngs = $referenceSizes.Count
    uniqueRuntimeGuids = $guids.Count
    manifestCoverage = 'passed'
    pngFormats = 'passed'
    importMetadata = 'passed'
    fontsAndLicenses = 'passed'
} | ConvertTo-Json
