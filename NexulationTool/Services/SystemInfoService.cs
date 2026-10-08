using System;
using System.Management;
using NexulationTool.Models;

namespace NexulationTool.Services
{
    public class SystemInfoService
    {
        public SystemSpecs GetSpecs()
        {
            var specs = new SystemSpecs();
            try
            {
                // OS Info
                string osName = Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 22000 
                    ? "Windows 11" : "Windows 10";
                specs.OSName = $"{osName} ({Environment.OSVersion.Version.Build})";

                // CPU Info
                using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string cpu = obj["Name"]?.ToString() ?? "CPU Detected";
                        cpu = cpu.Replace("Intel(R) Core(TM) ", "").Replace("AMD Ryzen ", "Ryzen ").Replace(" CPU", "");
                        specs.CPUName = cpu.Trim();
                        break;
                    }
                }

                // GPU Info
                using (var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string gpu = obj["Name"]?.ToString() ?? "GPU Detected";
                        if (!gpu.Contains("Basic Display"))
                        {
                            specs.GPUName = gpu.Replace("NVIDIA GeForce ", "").Replace("AMD Radeon ", "").Trim();
                            break;
                        }
                    }
                }

                // RAM Info
                using (var searcher = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        if (ulong.TryParse(obj["TotalPhysicalMemory"]?.ToString(), out ulong bytes))
                        {
                            double gb = Math.Round(bytes / (1024.0 * 1024.0 * 1024.0), 1);
                            specs.RAMInfo = $"{gb} GB RAM";
                        }
                        break;
                    }
                }
            }
            catch { }
            return specs;
        }
    }
}
