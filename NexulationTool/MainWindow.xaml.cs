using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using NexulationTool.Models;
using NexulationTool.Services;
using NexulationTool.Views;

namespace NexulationTool
{
    public partial class MainWindow : Window
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetTimerResolution(uint DesiredResolution, bool SetResolution, out uint CurrentResolution);

        private readonly SystemInfoService _systemInfoService = new SystemInfoService();
        private readonly DriverScanService _driverScanService = new DriverScanService();
        private readonly CleanerService _cleanerService = new CleanerService();

        private string _themeMode = "Auto"; // Auto, Dark, Light
        private string _activeNavTag = "Dashboard";

        public MainWindow()
        {
            InitializeComponent();
            CheckAdminRights();
            LoadSystemSpecs();
            RefreshPowerPlanInfo();
            RefreshLowLatencyStatus();
            RefreshDriverStatus();
            RefreshSettingsStatus();
            ApplyThemeMode("Auto");
        }

        private void CheckAdminRights()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                TxtStatus.Text = "Running without Administrator privileges. Some registry tweaks may fail.";
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Navigation & Smooth C# Animations
        private void BtnNav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                _activeNavTag = tag;
                UpdateNavButtonHighlights();

                // Hide all tabs
                TabDashboard.Visibility = Visibility.Collapsed;
                TabLatency.Visibility = Visibility.Collapsed;
                TabDiagnostics.Visibility = Visibility.Collapsed;
                TabCustomization.Visibility = Visibility.Collapsed;
                TabSettings.Visibility = Visibility.Collapsed;

                Grid targetTab = GetTabByTag(tag);
                if (targetTab != null)
                {
                    targetTab.Visibility = Visibility.Visible;
                    PlayTabAnimation(targetTab);
                }
            }
        }

        private Grid GetTabByTag(string tag)
        {
            switch (tag)
            {
                case "Dashboard": return TabDashboard;
                case "Latency": return TabLatency;
                case "Diagnostics": return TabDiagnostics;
                case "Customization": return TabCustomization;
                case "Settings": return TabSettings;
                default: return TabDashboard;
            }
        }

        private Button GetNavButtonByTag(string tag)
        {
            switch (tag)
            {
                case "Dashboard": return BtnNavDashboard;
                case "Latency": return BtnNavLatency;
                case "Diagnostics": return BtnNavDiagnostics;
                case "Customization": return BtnNavCustomization;
                case "Settings": return BtnNavSettings;
                default: return BtnNavDashboard;
            }
        }

        private void UpdateNavButtonHighlights()
        {
            ResetNavButton(BtnNavDashboard);
            ResetNavButton(BtnNavLatency);
            ResetNavButton(BtnNavDiagnostics);
            ResetNavButton(BtnNavCustomization);
            ResetNavButton(BtnNavSettings);

            Button activeBtn = GetNavButtonByTag(_activeNavTag);
            if (activeBtn != null)
            {
                activeBtn.SetResourceReference(Button.BackgroundProperty, "NavHoverBrush");
                activeBtn.SetResourceReference(Button.ForegroundProperty, "PrimaryTextBrush");
            }
        }

        private void ResetNavButton(Button btn)
        {
            if (btn == null) return;
            btn.ClearValue(Button.BackgroundProperty);
            btn.SetResourceReference(Button.ForegroundProperty, "SecondaryTextBrush");
        }

        private void PlayTabAnimation(Grid targetTab)
        {
            if (targetTab == null) return;

            DoubleAnimation fadeAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation slideAnim = new DoubleAnimation
            {
                From = 10,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            targetTab.BeginAnimation(UIElement.OpacityProperty, fadeAnim);

            if (targetTab.RenderTransform is TranslateTransform tt)
            {
                tt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            }
            else
            {
                TranslateTransform newTt = new TranslateTransform();
                targetTab.RenderTransform = newTt;
                newTt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
            }
        }
        #endregion

        #region Quick 1-Click System Boost
        private void BtnQuickBoost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NtSetTimerResolution(5000, true, out _);
                RunScript("Enable-MSIMode.ps1");
                RunScript("Set-LowPing.ps1");
                _cleanerService.TrimRAMWorkingSet();

                RefreshLowLatencyStatus();
                RefreshDriverStatus();
                TxtStatus.Text = "Quick System Boost applied: 0.5ms Timer, MSI Mode, & RAM trimmed.";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Quick Boost error: " + ex.Message;
            }
        }
        #endregion

        #region 3-Way Instant Theme Engine (ResourceDictionary Swapping)
        private void BtnThemeAuto_Click(object sender, RoutedEventArgs e)
        {
            ApplyThemeMode("Auto");
        }

        private void BtnThemeDark_Click(object sender, RoutedEventArgs e)
        {
            ApplyThemeMode("Dark");
        }

        private void BtnThemeLight_Click(object sender, RoutedEventArgs e)
        {
            ApplyThemeMode("Light");
        }

        private bool IsSystemLightTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        return Convert.ToInt32(key.GetValue("AppsUseLightTheme", 0)) == 1;
                    }
                }
            }
            catch { }
            return false;
        }

        private bool IsEffectiveThemeLight()
        {
            if (_themeMode == "Light") return true;
            if (_themeMode == "Dark") return false;
            return IsSystemLightTheme();
        }

        private void ApplyThemeMode(string mode)
        {
            _themeMode = mode;

            bool isLight = IsEffectiveThemeLight();
            ApplyThemeResourceDictionary(isLight);

            UpdateThemeButtonStates();
            UpdateNavButtonHighlights();

            TxtStatus.Text = $"Theme active: {(isLight ? "Clean Light" : "Fluent Dark")} ({(_themeMode == "Auto" ? "System Auto" : "Manual")}).";
        }

        private void ApplyThemeResourceDictionary(bool isLight)
        {
            try
            {
                string themeFile = isLight ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";
                Uri themeUri = new Uri(themeFile, UriKind.Relative);
                ResourceDictionary newTheme = (ResourceDictionary)Application.LoadComponent(themeUri);

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(newTheme);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Theme swap error: " + ex.Message);
            }
        }

        private void UpdateThemeButtonStates()
        {
            ResetThemeButton(BtnThemeAuto);
            ResetThemeButton(BtnThemeDark);
            ResetThemeButton(BtnThemeLight);

            if (_themeMode == "Auto") HighlightThemeButton(BtnThemeAuto);
            else if (_themeMode == "Dark") HighlightThemeButton(BtnThemeDark);
            else if (_themeMode == "Light") HighlightThemeButton(BtnThemeLight);
        }

        private void ResetThemeButton(Button btn)
        {
            if (btn == null) return;
            btn.SetResourceReference(Button.BackgroundProperty, "HeaderBgBrush");
            btn.SetResourceReference(Button.ForegroundProperty, "SecondaryTextBrush");
            btn.BorderThickness = new Thickness(1);
            btn.SetResourceReference(Button.BorderBrushProperty, "CardBorderBrush");
        }

        private void HighlightThemeButton(Button btn)
        {
            if (btn == null) return;
            btn.SetResourceReference(Button.BackgroundProperty, "AccentBrush");
            btn.Foreground = Brushes.White;
            btn.BorderThickness = new Thickness(0);
        }
        #endregion

        #region System Info Loader
        private void LoadSystemSpecs()
        {
            SystemSpecs specs = _systemInfoService.GetSpecs();
            TxtOS.Text = specs.OSName;
            TxtCPU.Text = specs.CPUName;
            TxtGPU.Text = specs.GPUName;
            TxtRAM.Text = specs.RAMInfo;
        }
        #endregion

        #region Power Plan Management
        private void RefreshPowerPlanInfo()
        {
            try
            {
                string output = RunCommand("powercfg", "/getactivescheme");
                if (!string.IsNullOrEmpty(output))
                {
                    if (output.Contains("Nexulation Power-Plan (Ryzen)") || output.Contains("Ryzen"))
                    {
                        TxtActivePlan.Text = "Nexulation Power-Plan (Ryzen)";
                        TxtHeroPowerPlan.Text = "Ryzen Power Plan";
                        TxtActivePlan.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    }
                    else if (output.Contains("Nexulation Power-Plan (Intel)") || output.Contains("Intel"))
                    {
                        TxtActivePlan.Text = "Nexulation Power-Plan (Intel)";
                        TxtHeroPowerPlan.Text = "Intel Power Plan";
                        TxtActivePlan.Foreground = new SolidColorBrush(Color.FromRgb(87, 162, 175));
                    }
                    else if (output.Contains("Ultimate Performance") || output.Contains("e9a42b02"))
                    {
                        TxtActivePlan.Text = "Ultimate Performance";
                        TxtHeroPowerPlan.Text = "Ultimate Power Plan";
                        TxtActivePlan.Foreground = new SolidColorBrush(Color.FromRgb(245, 158, 11));
                    }
                    else
                    {
                        Match m = Regex.Match(output, @"\((.+)\)");
                        string planName = m.Success ? m.Groups[1].Value : "Balanced / Default";
                        TxtActivePlan.Text = planName;
                        TxtHeroPowerPlan.Text = planName;
                        TxtActivePlan.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                    }
                }
            }
            catch
            {
                TxtActivePlan.Text = "Active Scheme Detected";
            }
        }

        private void BtnApplyRyzenPlan_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Set-AMD-PowerPlan.ps1");
            RefreshPowerPlanInfo();
            TxtStatus.Text = "Applied Nexulation Power-Plan (Ryzen).";
        }

        private void BtnApplyIntelPlan_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Set-Intel-PowerPlan.ps1");
            RefreshPowerPlanInfo();
            TxtStatus.Text = "Applied Nexulation Power-Plan (Intel).";
        }

        private void BtnApplyUltimatePlan_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Set-UltimatePowerPlan.ps1");
            RefreshPowerPlanInfo();
            TxtStatus.Text = "Applied Ultimate Performance Plan.";
        }
        #endregion

        #region Driver DPC/ISR & MSI Status Scanner
        private void RefreshDriverStatus()
        {
            DriverStatusInfo info = _driverScanService.ScanDriverStatuses();
            SetDriverBadge(TxtStatusGPU, info.GPUStatus);
            SetDriverBadge(TxtStatusNet, info.NetStatus);
            SetDriverBadge(TxtStatusNVMe, info.NVMeStatus);
            SetDriverBadge(TxtStatusUSB, info.USBStatus);
        }

        private void SetDriverBadge(TextBlock txt, string status)
        {
            txt.Text = status;
            txt.Foreground = status.Contains("ACTIVE")
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129))
                : new SolidColorBrush(Color.FromRgb(239, 68, 68));
        }

        private void BtnRefreshDriverStatus_Click(object sender, RoutedEventArgs e)
        {
            RefreshDriverStatus();
            TxtStatus.Text = "Re-scanned active DPC/ISR driver MSI modes.";
        }
        #endregion

        #region Latency & Mouse Diagnostics
        private void BtnRunXperfTrace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtStatus.Text = "Running 10-second DPC/ISR xperf trace...";

                System.Threading.Tasks.Task.Run(() =>
                {
                    string traceFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "nexulation_dpc_latency.etl");
                    RunCommand("xperf", "-on PROC_THREAD+LOADER+DPC+INTERRUPT+CSWITCH -stackwalk CSwitch+DPC+Interrupt -BufferSize 1024 -MaxBuffers 1024");
                    System.Threading.Thread.Sleep(10000);
                    RunCommand("xperf", $"-stop -d \"{traceFile}\"");

                    Dispatcher.Invoke(() =>
                    {
                        if (File.Exists(traceFile))
                        {
                            TxtStatus.Text = $"xperf ETL saved to Desktop: {traceFile}";
                            RunCommand("wpa", $"\"{traceFile}\"");
                        }
                        else
                        {
                            TxtStatus.Text = "xperf trace completed. (WPA / WPT optional)";
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "xperf error: " + ex.Message;
            }
        }

        private void BtnLaunchMouseTester_Click(object sender, RoutedEventArgs e)
        {
            MouseTesterWindow win = new MouseTesterWindow();
            win.Owner = this;
            win.ShowDialog();
        }
        #endregion

        #region Low Latency Toggles
        private void RefreshLowLatencyStatus()
        {
            bool msiOn = _driverScanService.ScanDriverStatuses().GPUStatus.Contains("ACTIVE");
            ChkToggleMSI.IsChecked = msiOn;

            bool pingOn = CheckLowPing();
            ChkTogglePing.IsChecked = pingOn;

            bool memCompOn = CheckMemoryCompression();
            ChkToggleMemComp.IsChecked = memCompOn;

            bool hypervisorOff = CheckHypervisorOff();
            ChkToggleHypervisor.IsChecked = hypervisorOff;

            bool kbOn = CheckKeyboardSpeed();
            ChkToggleKeyboard.IsChecked = kbOn;

            RefreshTimerResolutionUI();
        }

        private void RefreshTimerResolutionUI()
        {
            try
            {
                NtQueryTimerResolution(out _, out _, out uint current);
                double currentMs = (double)current / 10000.0;
                TxtTimerResInfo.Text = $"Current Resolution: {currentMs:F3} ms";
            }
            catch { }
        }

        private void BtnToggleTimerRes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NtSetTimerResolution(5000, true, out uint current);
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Kernel"))
                {
                    if (key != null)
                    {
                        key.SetValue("GlobalTimerResolutionRequests", 1, RegistryValueKind.DWord);
                    }
                }

                RefreshTimerResolutionUI();
                TxtStatus.Text = "Locked System Timer Resolution to 0.500 ms (500 μs).";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Timer error: " + ex.Message;
            }
        }

        private void ChkToggleMSI_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Enable-MSIMode.ps1");
            RefreshLowLatencyStatus();
            RefreshDriverStatus();
            TxtStatus.Text = "PCI MSI Mode prioritized for GPU, USB, & Net.";
        }

        private bool CheckLowPing()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"))
                {
                    if (key != null)
                    {
                        foreach (string sub in key.GetSubKeyNames())
                        {
                            using (RegistryKey iface = key.OpenSubKey(sub))
                            {
                                if (iface != null && Convert.ToInt32(iface.GetValue("TcpAckFrequency", 0)) == 1)
                                    return true;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private void ChkTogglePing_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Set-LowPing.ps1");
            RefreshLowLatencyStatus();
            TxtStatus.Text = "Low-Ping TCP packet tuning applied.";
        }

        private bool CheckMemoryCompression()
        {
            string res = RunPowerShell("Get-MMAgent | Select-Object -ExpandProperty MemoryCompression");
            return res.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        private void ChkToggleMemComp_Click(object sender, RoutedEventArgs e)
        {
            bool current = CheckMemoryCompression();
            if (current)
            {
                RunPowerShell("Disable-MMAgent -MemoryCompression");
                TxtStatus.Text = "Memory Compression Disabled (CPU cycles saved).";
            }
            else
            {
                RunPowerShell("Enable-MMAgent -MemoryCompression");
                TxtStatus.Text = "Memory Compression Enabled.";
            }
            RefreshLowLatencyStatus();
        }

        private bool CheckHypervisorOff()
        {
            string outBcd = RunCommand("bcdedit", "/enum {current}");
            return outBcd.Contains("hypervisorlaunchtype") && outBcd.Contains("Off");
        }

        private void ChkToggleHypervisor_Click(object sender, RoutedEventArgs e)
        {
            bool currentOff = CheckHypervisorOff();
            if (!currentOff)
            {
                RunCommand("bcdedit", "/set hypervisorlaunchtype off");
                TxtStatus.Text = "Hypervisor set to OFF (Requires reboot for bare-metal CPU).";
            }
            else
            {
                RunCommand("bcdedit", "/set hypervisorlaunchtype auto");
                TxtStatus.Text = "Hypervisor set to Auto.";
            }
            RefreshLowLatencyStatus();
        }

        private bool CheckKeyboardSpeed()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Keyboard"))
                {
                    if (key != null)
                    {
                        string delay = key.GetValue("KeyboardDelay")?.ToString();
                        string speed = key.GetValue("KeyboardSpeed")?.ToString();
                        return delay == "0" && speed == "31";
                    }
                }
            }
            catch { }
            return false;
        }

        private void ChkToggleKeyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Keyboard", true))
                {
                    if (key != null)
                    {
                        key.SetValue("KeyboardDelay", "0", RegistryValueKind.String);
                        key.SetValue("KeyboardSpeed", "31", RegistryValueKind.String);
                    }
                }
                TxtStatus.Text = "Keyboard Max Repeat Rate applied (Delay=0, Speed=31).";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Error: " + ex.Message;
            }
            RefreshLowLatencyStatus();
        }
        #endregion

        #region Clean & RAM Optimization
        private void BtnCleanTemp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = _cleanerService.CleanTempCaches();
                TxtStatus.Text = $"Cleaned {count} temporary files and caches.";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Clean error: " + ex.Message;
            }
        }

        private void BtnTrimRAM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _cleanerService.TrimRAMWorkingSet();
                TxtStatus.Text = "System RAM Working Sets trimmed.";
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "RAM trim error: " + ex.Message;
            }
        }
        #endregion

        #region Settings Management
        private void RefreshSettingsStatus()
        {
            bool autoRunOn = CheckAutoRun();
            ChkToggleAutoRun.IsChecked = autoRunOn;
        }

        private bool CheckAutoRun()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                {
                    return key?.GetValue("NexulationTool") != null;
                }
            }
            catch { return false; }
        }

        private void ChkToggleAutoRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        bool current = CheckAutoRun();
                        if (current)
                        {
                            key.DeleteValue("NexulationTool", false);
                            TxtStatus.Text = "Disabled Windows Auto-Run.";
                        }
                        else
                        {
                            string exePath = Process.GetCurrentProcess().MainModule.FileName;
                            key.SetValue("NexulationTool", $"\"{exePath}\"");
                            TxtStatus.Text = "Enabled Windows Auto-Run on Startup.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TxtStatus.Text = "Auto-run error: " + ex.Message;
            }
            RefreshSettingsStatus();
        }

        private void ChkToggleAutoTimer_Click(object sender, RoutedEventArgs e)
        {
            BtnToggleTimerRes_Click(sender, e);
            TxtStatus.Text = "0.5ms Timer Auto-Lock configured.";
        }

        private void BtnApplyWallpaper_Click(object sender, RoutedEventArgs e)
        {
            RunScript("Set-Wallpaper.ps1");
            TxtStatus.Text = "Applied official Nexulation desktop wallpaper.";
        }
        #endregion

        #region Helper Runner
        private string RunCommand(string cmd, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output;
                }
            }
            catch { return ""; }
        }

        private string RunPowerShell(string script)
        {
            return RunCommand("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"");
        }

        private void RunScript(string scriptName)
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Playbook", "Executables", scriptName);
            if (!File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Executables", scriptName);
            }

            if (File.Exists(scriptPath))
            {
                RunPowerShell($"& '{scriptPath}'");
            }
            else
            {
                TxtStatus.Text = $"Script missing: {scriptName}";
            }
        }
        #endregion
    }
}