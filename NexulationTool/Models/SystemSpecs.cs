namespace NexulationTool.Models
{
    public class SystemSpecs
    {
        public string OSName { get; set; } = "Windows 11";
        public string CPUName { get; set; } = "Detecting CPU...";
        public string GPUName { get; set; } = "Detecting GPU...";
        public string RAMInfo { get; set; } = "Detecting RAM...";
    }

    public class DriverStatusInfo
    {
        public string GPUStatus { get; set; } = "Checking...";
        public string NetStatus { get; set; } = "Checking...";
        public string NVMeStatus { get; set; } = "Checking...";
        public string USBStatus { get; set; } = "Checking...";
    }
}
