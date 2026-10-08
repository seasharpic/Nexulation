# Install Nexulation Tool to C:\Program Files\NexulationTool and create shortcuts
$ErrorActionPreference = "SilentlyContinue"

$installDir = "C:\Program Files\NexulationTool"
if (-not (Test-Path $installDir)) {
    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
}

$sourceDir = "$PSScriptRoot\NexulationTool"
if (Test-Path $sourceDir) {
    Copy-Item -Path "$sourceDir\*" -Destination $installDir -Recurse -Force
}

# Create Desktop Shortcut
$desktopPath = [System.IO.Path]::Combine([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Desktop), "Nexulation Tool.lnk")
$targetPath = "$installDir\NexulationTool.exe"

if (Test-Path $targetPath) {
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($desktopPath)
    $Shortcut.TargetPath = $targetPath
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "Nexulation Low-Latency Companion Tool"
    $Shortcut.Save()
}
