using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NexulationTool.Views
{
    public partial class MouseTesterWindow : Window
    {
        private long _lastTicks = 0;
        private readonly List<double> _intervalsMs = new List<double>();
        private readonly Stopwatch _testTimer = new Stopwatch();
        private long _totalPackets = 0;

        public MouseTesterWindow()
        {
            InitializeComponent();
            _testTimer.Start();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TestArea_MouseMove(object sender, MouseEventArgs e)
        {
            long currentTicks = Stopwatch.GetTimestamp();

            if (_lastTicks > 0)
            {
                double elapsedMs = (double)(currentTicks - _lastTicks) * 1000.0 / Stopwatch.Frequency;
                
                // Filter out invalid zero or huge idle intervals
                if (elapsedMs > 0.05 && elapsedMs < 50.0)
                {
                    _totalPackets++;
                    _intervalsMs.Add(elapsedMs);

                    if (_intervalsMs.Count > 200)
                    {
                        _intervalsMs.RemoveAt(0);
                    }

                    UpdateStats();
                }
            }

            _lastTicks = currentTicks;
        }

        private void UpdateStats()
        {
            if (_intervalsMs.Count == 0) return;

            double avgMs = _intervalsMs.Average();
            double minMs = _intervalsMs.Min();
            double maxMs = _intervalsMs.Max();
            double currentHz = avgMs > 0 ? (1000.0 / avgMs) : 0;

            TxtHz.Text = $"{Math.Round(currentHz)} Hz";
            TxtAvgInterval.Text = $"{avgMs:F3} ms";
            TxtMinMaxInterval.Text = $"{minMs:F2} / {maxMs:F2} ms";
            TxtPackets.Text = _totalPackets.ToString();

            if (currentHz >= 7000)
                TxtHz.Foreground = System.Windows.Media.Brushes.Magenta;
            else if (currentHz >= 3500)
                TxtHz.Foreground = System.Windows.Media.Brushes.Cyan;
            else if (currentHz >= 900)
                TxtHz.Foreground = System.Windows.Media.Brushes.SpringGreen;
            else
                TxtHz.Foreground = System.Windows.Media.Brushes.Yellow;
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _intervalsMs.Clear();
            _totalPackets = 0;
            _lastTicks = 0;
            TxtHz.Text = "0 Hz";
            TxtAvgInterval.Text = "0.00 ms";
            TxtMinMaxInterval.Text = "0.0 / 0.0 ms";
            TxtPackets.Text = "0";
        }
    }
}
