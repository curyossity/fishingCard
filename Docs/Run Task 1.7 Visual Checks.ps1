param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.5.9f1/Editor/Unity.exe')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'Temp/Task17VisualChecks'
$output = Join-Path $root 'Logs/Task17VisualChecks'
New-Item -ItemType Directory -Force -Path "$project/Assets/Editor", "$project/Assets/FishingUIAssets", "$project/Packages", "$project/ProjectSettings", $output | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot/Task17VisualChecks.cs" -Destination "$project/Assets/Editor/Task17VisualChecks.cs"
Copy-Item -LiteralPath "$PSScriptRoot/Task 1.7 Image Generation.json" -Destination "$project/Assets/record.json"
Copy-Item -LiteralPath "$root/ProjectSettings/ProjectVersion.txt" -Destination "$project/ProjectSettings/ProjectVersion.txt"
Copy-Item -LiteralPath "$PSScriptRoot/Task17ValidationPackages.json" -Destination "$project/Packages/manifest.json"
foreach ($folder in @('Icons', 'Overlays')) {
    Copy-Item -LiteralPath "$root/Assets/FishingUIAssets/$folder" -Destination "$project/Assets/FishingUIAssets" -Recurse -Force
}
$arguments = @('-batchmode', '-projectPath', ('"' + $project + '"'), '-executeMethod', 'Task17VisualChecks.Run', '-validationOutput', ('"' + $output + '"'), '-logFile', ('"' + "$output/editor.log" + '"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$process.WaitForExit(300000)) {
    $process.Kill()
    $process.WaitForExit()
    throw "Isolated Unity validation timed out. See $output/editor.log"
}
if ($process.ExitCode -ne 0) { throw "Unity validation failed ($($process.ExitCode)). See $output/editor.log" }
Get-Content "$output/result.json"
