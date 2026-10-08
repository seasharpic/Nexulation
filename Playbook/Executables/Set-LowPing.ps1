# Enable Low-Ping TCP Network Tuning (Disables Nagle's Algorithm)
$ErrorActionPreference = "SilentlyContinue"

$interfacesPath = "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"
if (Test-Path $interfacesPath) {
    Get-ChildItem -Path $interfacesPath | ForEach-Object {
        Set-ItemProperty -Path $_.PSPath -Name "TcpAckFrequency" -Value 1 -Type DWord
        Set-ItemProperty -Path $_.PSPath -Name "TCPNoDelay" -Value 1 -Type DWord
        Set-ItemProperty -Path $_.PSPath -Name "TcpDelAckTicks" -Value 0 -Type DWord
    }
}

# Set Global TCPIP Parameters
$tcpGlobalPath = "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters"
if (Test-Path $tcpGlobalPath) {
    Set-ItemProperty -Path $tcpGlobalPath -Name "DefaultTTL" -Value 64 -Type DWord
    Set-ItemProperty -Path $tcpGlobalPath -Name "EnableDCA" -Value 1 -Type DWord
}
