<div align="center">
  <h1>Nexulation</h1>
  <p>Minimal, low-latency Playbook for AME Wizard (Windows 10 22H2 & 11 22H2+)</p>
</div>

---

## ⚡ What is Nexulation?

**Nexulation** is a lightweight AME Wizard Playbook focused on reducing latency, improving FPS stability, and removing Windows bloatware for competitive titles like **CS2, Fortnite, and Valorant**.

Instead of randomly deleting system files, it focuses on tested low-latency tweaks while keeping Windows stable.

---

## 🛠️ Main Tweaks & Optimizations

- **Win32PrioritySeparation (0x18 / 24)** — 1:1 quantum ratio between system and active window to eliminate frame drops.
- **MSI Mode (Message Signaled Interrupts)** — Enabled for GPU (High Priority), USB, Network, and NVMe controllers to lower DPC/ISR latency.
- **Ultimate Performance Plan & No Core Parking** — Prevents CPU frequency drops and micro-stutters.
- **Native MPO / Flip Model** — Keeps Windows 10/11 Flip Model intact without forcing broken FSO/FSE modes.
- **HVCI / Memory Integrity Disabled** — Registry tweak to boost FPS in competitive games.
- **Keyboard Repeat Rate** — Set to `KeyboardDelay = 0` and `KeyboardSpeed = 31` for responsive key repeat.
- **Storage & NTFS Tweaks** — Disables `disablelastaccess` & `disable8dot3`, enables SSD TRIM, disables background defrag.
- **Autologger Disabling** — Turns off background WMI Autologger tracing sessions (`DiagTrack`, `Circular Kernel`).
- **Custom Wallpapers & Utility Options** — Includes custom dark/light wallpapers and optional installs for Process Lasso, MSI Afterburner, LatencyMon, 7-Zip, Notepad++.

---

## 📥 How to Install

1. Download `Nexulation.apbx` from [Releases](https://github.com/dyagyatis/Nexulation/releases).
2. Download and launch [AME Wizard](https://download.ameliorated.io/AME%20Beta.zip).
3. Drag and drop `Nexulation.apbx` into AME Wizard.
4. Select your preferred options and follow on-screen steps.
5. Reboot after completion.

---

## ⚙️ Building from Source

```bash
git clone https://github.com/dyagyatis/Nexulation.git
cd Nexulation
BUILD.bat
```

---

Created by **dyagyatis**
