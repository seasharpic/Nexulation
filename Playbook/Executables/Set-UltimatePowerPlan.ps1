# Configure Ultimate Performance Power Plan & Disable Core Parking
$ErrorActionPreference = "SilentlyContinue"

# 1. Unhide / Duplicate Ultimate Performance Plan
$ultGuid = "e9a42b02-d5df-448d-aa00-03f14749eb61"
$highGuid = "8c527fac-351d-4878-8f3b-54099630735b"

# Duplicate Ultimate Performance plan if not present
$plans = powercfg /l
if ($plans -notmatch $ultGuid) {
    powercfg -duplicatescheme $ultGuid | Out-Null
}

# Get list of plans again
$plans = powercfg /l
$targetGuid = $ultGuid

if ($plans -notmatch $ultGuid) {
    $targetGuid = $highGuid
}

# 2. Set active scheme
powercfg /setactive $targetGuid

# 3. Disable Core Parking & Set Min/Max CPU State to 100%
# SUB_PROCESSOR (54533251-82be-4824-96c1-47b60b740d00)
$subProc = "54533251-82be-4824-96c1-47b60b740d00"

# PROCTHROTTLEMIN (893de362-ede9-464a-be45-0b30afe73228) -> 100%
powercfg /setacvalueindex SCHEME_CURRENT $subProc 893de362-ede9-464a-be45-0b30afe73228 100

# PROCTHROTTLEMAX (bc50e329-59f1-4632-9500-0d7258cda0e0) -> 100%
powercfg /setacvalueindex SCHEME_CURRENT $subProc bc50e329-59f1-4632-9500-0d7258cda0e0 100

# CPMINCORES (0cc5b647-c1df-4596-92da-07c13696167a) -> 100% (Disables Core Parking)
powercfg /setacvalueindex SCHEME_CURRENT $subProc 0cc5b647-c1df-4596-92da-07c13696167a 100

# Apply active scheme
powercfg /setactive SCHEME_CURRENT
