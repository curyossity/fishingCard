param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.5.9f1/Editor/Unity.exe')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'Temp/Task18VisualChecks'
$output = Join-Path $root 'Logs/Task18VisualChecks'
New-Item -ItemType Directory -Force -Path "$project/Assets/Editor", "$project/Assets/FishingUIAssets", "$project/Packages", "$project/ProjectSettings", $output | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot/Task17VisualChecks.cs" -Destination "$project/Assets/Editor/Task17VisualChecks.cs"
Copy-Item -LiteralPath "$PSScriptRoot/Task18PackageChecks.cs" -Destination "$project/Assets/Editor/Task18PackageChecks.cs"
Copy-Item -LiteralPath "$PSScriptRoot/Task 1.7 Image Generation.json" -Destination "$project/Assets/record.json"
Copy-Item -LiteralPath "$root/ProjectSettings/ProjectVersion.txt" -Destination "$project/ProjectSettings/ProjectVersion.txt"
Copy-Item -LiteralPath "$PSScriptRoot/Task17ValidationPackages.json" -Destination "$project/Packages/manifest.json"
New-Item -ItemType Directory -Force -Path "$project/Assets/ReferenceChecks" | Out-Null
foreach ($name in @('component-dimensions', 'gameplay-layout-1920x1080', 'gameplay-layout-blank-1920x1080', 'theme-palette', 'typography-reference', 'ui-component-contact-sheet', 'visual-states')) {
    Copy-Item -LiteralPath "$root/Assets/FishingUIAssets/Reference/$name.png", "$root/Assets/FishingUIAssets/Reference/$name.png.meta" -Destination "$project/Assets/ReferenceChecks" -Force
}
Copy-Item -LiteralPath "$root/Assets/FishingUIAssets/Fonts" -Destination "$project/Assets/FishingUIAssets" -Recurse -Force
foreach ($folder in @('Backgrounds', 'Cards', 'Controls', 'Effects', 'Frames', 'Icons', 'Markers', 'Meters', 'Overlays', 'Rig')) {
    Copy-Item -LiteralPath "$root/Assets/FishingUIAssets/$folder" -Destination "$project/Assets/FishingUIAssets" -Recurse -Force
}
$temporarySettings = Join-Path $project 'Assets/Resources/TMP Settings.asset'
if (Test-Path -LiteralPath $temporarySettings) {
    Remove-Item -LiteralPath $temporarySettings, ($temporarySettings + '.meta') -Force
}
function Invoke-ValidationUnity([string]$method, [string]$logName) {
    $arguments = @('-batchmode', '-projectPath', ('"' + $project + '"'), '-executeMethod', $method, '-validationOutput', ('"' + $output + '"'), '-logFile', ('"' + (Join-Path $output $logName) + '"'))
    $process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (!$process.WaitForExit(300000)) {
        $process.Kill()
        $process.WaitForExit()
        throw "Isolated Unity validation timed out. See $(Join-Path $output $logName)"
    }
    if ($process.ExitCode -ne 0) { throw "Unity validation failed ($($process.ExitCode)). See $(Join-Path $output $logName)" }
}
Invoke-ValidationUnity 'Task17VisualChecks.PrepareTmp' 'tmp-import.log'
Invoke-ValidationUnity 'Task17VisualChecks.RunPackage' 'editor.log'
Get-Content "$output/result.json"
