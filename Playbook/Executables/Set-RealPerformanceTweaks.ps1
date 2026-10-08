# Real Performance & Low-Latency Tweaks
# 1. Disable Hypervisor BCD Launch
# 2. Disable Memory Compression & Page Combining
# 3. Optimize Netsh TCP/IP parameters
$ErrorActionPreference = "SilentlyContinue"

# 1. Disable Hypervisor in BCD
bcdedit /set hypervisorlaunchtype off | Out-Null

# 2. Disable Memory Compression & Page Combining (Frees CPU cycles for games)
Disable-MMAgent -MemoryCompression -PageCombining

# 3. Netsh TCP/IP Stack Low-Latency Tuning
netsh int tcp set global ecncapability=disabled | Out-Null
netsh int tcp set global timestamps=disabled | Out-Null
netsh int tcp set global autotuninglevel=normal | Out-Null
netsh int tcp set global rsc=disabled | Out-Null
