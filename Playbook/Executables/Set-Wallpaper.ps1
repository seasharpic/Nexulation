param (
    [string]$WallpaperFile = ""
)

$ErrorActionPreference = "SilentlyContinue"

# 1. Determine active theme if WallpaperFile not explicitly passed
if ([string]::IsNullOrEmpty($WallpaperFile)) {
    $themeReg = Get-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'AppsUseLightTheme' -ErrorAction SilentlyContinue
    if ($themeReg.AppsUseLightTheme -eq 1) {
        $WallpaperFile = "NexulationLight.png"
    } else {
        $WallpaperFile = "NexulationDark.png"
    }
}

# 2. Determine paths
$ScriptDir = $PSScriptRoot
$SourcePath = Join-Path $ScriptDir "Wallpaper\$WallpaperFile"

if (-not (Test-Path $SourcePath)) {
    # Fallback search if called from root Executables
    $SourcePath = "Executables\Wallpaper\$WallpaperFile"
}

if (-not (Test-Path $SourcePath)) {
    $SourcePath = Join-Path $ScriptDir "Wallpaper\NexulationDark.png"
}

$DestDir = "C:\Windows\Web\Wallpaper"
if (-not (Test-Path $DestDir)) {
    New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
}

$DestPath = Join-Path $DestDir "Nexulation-Wallpaper.png"

# 3. Copy Wallpaper file
if (Test-Path $SourcePath) {
    Copy-Item -Path $SourcePath -Destination $DestPath -Force
}

# 4. Update Registry
Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'Wallpaper' -Value $DestPath
Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'WallpaperStyle' -Value '10' # Fill
Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'TileWallpaper' -Value '0'

# 5. Clear cached wallpaper to prevent stale cache
$TranscodedPath = "$env:APPDATA\Microsoft\Windows\Themes\TranscodedWallpaper"
if (Test-Path $TranscodedPath) {
    Remove-Item $TranscodedPath -Force
}

# 6. Native Win32 SystemParametersInfo Call (SPI_SETDESKWALLPAPER = 0x0014)
$code = @"
using System;
using System.Runtime.InteropServices;

public class Win32Api {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
"@

Add-Type -TypeDefinition $code -ErrorAction SilentlyContinue
[Win32Api]::SystemParametersInfo(0x0014, 0, $DestPath, 0x01 -bor 0x02)
