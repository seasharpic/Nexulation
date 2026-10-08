using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace NexulationTool.Services
{
    public class CleanerService
    {
        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public int CleanTempCaches()
        {
            int count = 0;
            string userTemp = Path.GetTempPath();
            string winTemp = @"C:\Windows\Temp";

            count += CleanFolder(userTemp);
            count += CleanFolder(winTemp);
            return count;
        }

        private int CleanFolder(string path)
        {
            int deleted = 0;
            if (!Directory.Exists(path)) return 0;

            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    file.Delete();
                    deleted++;
                }
                catch { }
            }
            return deleted;
        }

        public void TrimRAMWorkingSet()
        {
            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                try
                {
                    EmptyWorkingSet(p.Handle);
                }
                catch { }
            }
        }
    }
}
