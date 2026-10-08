# Nexulation Intel SpeedShift Max Power Plan
# Tailored specifically for Intel Core architecture (SpeedShift HWP, P-Core Priority, PCIe, C-States, Latency Hints)
$ErrorActionPreference = "SilentlyContinue"

# 1. Duplicate / Create Custom Plan with Unique Name "Nexulation Intel SpeedShift Max"
$ultGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61"
$highGuid = "8c527fac-351d-4878-8f3b-54099630735b"

$plans = powercfg /l
if ($plans -notmatch $ultGuid) { powercfg -duplicatescheme $ultGuid | Out-Null }
$plans = powercfg /l
$targetGuid = if ($plans -match $ultGuid) { $ultGuid } else { $highGuid }

powercfg /setactive $targetGuid
powercfg /changename SCHEME_CURRENT "Nexulation Power-Plan (Intel)" "Custom low-latency power plan for Intel Core processors."

$subProc = "54533251-82be-4824-96c1-47b60b740d00"
$subPci  = "5038fc98-9780-4564-98af-9f9f87915717"
$subDisk = "0012ee47-9041-4b5d-9b77-535fba8b1442"
$subUsb  = "2a737441-1930-4402-8d77-b2bebba308a3"

# --- PROCESSOR SETTINGS ---
# Min / Max CPU State -> 100%
powercfg /setacvalueindex SCHEME_CURRENT $subProc 893de362-ede9-464a-be45-0b30afe73228 100
powercfg /setacvalueindex SCHEME_CURRENT $subProc bc50e329-59f1-4632-9500-0d7258cda0e0 100

# Disable Core Parking (100%)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 0cc5b647-c1df-4596-92da-07c13696167a 100

# Intel SpeedShift Decrease Threshold -> 100% (Blocks frequency drop latency)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 12a5988e-b75b-4d0b-8785-8869c3b6a9e4 100

# Intel HWP Energy Performance Preference -> 0 (0% Energy / Max Clock)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 36687f9e-137c-46a2-813c-36320a6470f1 0

# Heterogeneous Thread Scheduling -> Prefer Performant Cores (P-Cores) (1)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 93b8b6dc-0698-4d1c-9ee4-0644e900c85d 1

# Short Thread Scheduling Policy -> Prefer Performant Cores (P-Cores) (1)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 7f24be0d-c6e1-4f9c-9b19-504a8f58b285 1

# System Cooling Policy -> Active (1)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 08902b77-d7e6-4158-99a2-517405d4d1e0 1

# Latency Sensitivity Hint Min & Max -> 100% (Instant response to game input events)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 616c6256-414c-482b-a45b-f06800d4b68e 100
powercfg /setacvalueindex SCHEME_CURRENT $subProc cc5b4420-ae75-47cb-9a69-aa7849819232 100

# --- PERIPHERAL & BUS POWER MANAGEMENT ---
# PCIe Link State Power Management -> Off (0)
powercfg /setacvalueindex SCHEME_CURRENT $subPci 2a5f94d0-c920-4284-850f-90e633a6934c 0

# Disk Turn Off -> Never (0)
powercfg /setacvalueindex SCHEME_CURRENT $subDisk 6738e2c4-e8a5-4a42-b16a-e471e71b3f8e 0

# USB Selective Suspend -> Disabled (0)
powercfg /setacvalueindex SCHEME_CURRENT $subUsb 48e2824b-3381-492d-b0c9-2d67d1864073 0

powercfg /setactive SCHEME_CURRENT
