# Enable MSI (Message Signaled Interrupts) Mode for GPU, USB, Network, and NVMe Controllers
$ErrorActionPreference = "SilentlyContinue"

$pciKeyPath = "HKLM:\SYSTEM\CurrentControlSet\Enum\PCI"
if (-not (Test-Path $pciKeyPath)) { exit }

# Get all PCI device keys
$devices = Get-ChildItem -Path $pciKeyPath -Recurse | Where-Object { $_.PSChildName -eq "Device Parameters" }

foreach ($devParam in $devices) {
    $parent = Get-ItemProperty -Path $devParam.PSPath -ErrorAction SilentlyContinue
    $class = (Get-ItemProperty -Path (Split-Path $devParam.PSPath -Parent) -ErrorAction SilentlyContinue).Class
    
    # Target Classes: Display (GPU), USB, Net, Media, SCSIAdapter (NVMe)
    if ($class -in @("Display", "USB", "Net", "SCSIAdapter", "Media")) {
        $msiPath = Join-Path $devParam.PSPath "Interrupt Management\MessageSignaledInterruptProperties"
        if (-not (Test-Path $msiPath)) {
            New-Item -Path $msiPath -Force | Out-Null
        }
        Set-ItemProperty -Path $msiPath -Name "MSISupported" -Value 1 -Type DWord
        
        # High Priority for Display (GPU) interrupts
        if ($class -eq "Display") {
            $affPath = Join-Path $devParam.PSPath "Interrupt Management\Affinity Policy"
            if (-not (Test-Path $affPath)) {
                New-Item -Path $affPath -Force | Out-Null
            }
            Set-ItemProperty -Path $affPath -Name "DevicePriority" -Value 3 -Type DWord # 3 = High
        }
    }
}
