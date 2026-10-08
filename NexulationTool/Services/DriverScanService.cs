using System;
using Microsoft.Win32;
using NexulationTool.Models;

namespace NexulationTool.Services
{
    public class DriverScanService
    {
        public DriverStatusInfo ScanDriverStatuses()
        {
            var info = new DriverStatusInfo();
            try
            {
                info.GPUStatus = CheckMSIModeForClass("{4d36e968-e325-11ce-bfc1-08002be10318}") ? "MSI ACTIVE (High Priority)" : "Line-Based Interrupt (Legacy)";
                info.NetStatus = CheckMSIModeForClass("{4d36e972-e325-11ce-bfc1-08002be10318}") ? "MSI ACTIVE" : "Line-Based Interrupt (Legacy)";
                info.NVMeStatus = CheckMSIModeForClass("{4d36e97b-e325-11ce-bfc1-08002be10318}") ? "MSI ACTIVE" : "Line-Based Interrupt (Legacy)";
                info.USBStatus = CheckMSIModeForClass("{36fc9e60-c465-11cf-8056-444553540000}") ? "MSI ACTIVE" : "Line-Based Interrupt (Legacy)";
            }
            catch { }
            return info;
        }

        private bool CheckMSIModeForClass(string classGuid)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Control\Class\{classGuid}"))
                {
                    if (key != null)
                    {
                        foreach (string subkeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey sub = key.OpenSubKey($"{subkeyName}\\Device Parameters\\Interrupt Management\\MessageSignaledInterruptProperties"))
                            {
                                if (sub != null && Convert.ToInt32(sub.GetValue("MSISupported", 0)) == 1)
                                    return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
