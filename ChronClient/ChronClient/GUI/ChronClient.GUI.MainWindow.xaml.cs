﻿using Chrones.Cmr;
using System;
using System.Windows;
using System.Windows.Threading;
using ChronClient.Data;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using Chrones.Cmr.Imports;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ChronClient.GUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer SlowUpdateGUI;

        #region MaximizingFix
        CompositionTarget WindowCompositionTarget { get; set; }

        double CachedMinWidth { get; set; }

        double CachedMinHeight { get; set; }

        Import.POINT CachedMinTrackSize { get; set; }

        IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    Import.MINMAXINFO mmi = (Import.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Import.MINMAXINFO));
                    IntPtr monitor = Import.MonitorFromWindow(hwnd, 0x00000002 /*MONITOR_DEFAULTTONEAREST*/);
                    if (monitor != IntPtr.Zero)
                    {
                        Import.MONITORINFO monitorInfo = new Import.MONITORINFO { };
                        Import.GetMonitorInfo(monitor, monitorInfo);
                        Import.RECT rcWorkArea = monitorInfo.rcWork;
                        Import.RECT rcMonitorArea = monitorInfo.rcMonitor;
                        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                        if (!CachedMinTrackSize.Equals(mmi.ptMinTrackSize) || CachedMinHeight != MinHeight && CachedMinWidth != MinWidth)
                        {
                            mmi.ptMinTrackSize.x = (int)((CachedMinWidth = MinWidth) * WindowCompositionTarget.TransformToDevice.M11);
                            mmi.ptMinTrackSize.y = (int)((CachedMinHeight = MinHeight) * WindowCompositionTarget.TransformToDevice.M22);
                            CachedMinTrackSize = mmi.ptMinTrackSize;
                        }
                    }
                    Marshal.StructureToPtr(mmi, lParam, true);
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion


        public MainWindow()
        {
            InitializeComponent();

            #region Timer
            SlowUpdateGUI = new DispatcherTimer();
            SlowUpdateGUI.Interval = new TimeSpan(100);
            SlowUpdateGUI.Tick += SlowUpdateGUI_Tick;
            SlowUpdateGUI.Start();
            #endregion

            #region MaximizingFix
            SourceInitialized += (s, e) =>
            {
                WindowCompositionTarget = PresentationSource.FromVisual(this).CompositionTarget;
                HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(WindowProc);
            };
            #endregion
        }

        private void Control_Close_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
            cmr.ExitApplication();
        }

        private void Control_Maximize_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            } else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Control_Minimize_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    
        private void SlowUpdateGUI_Tick(object sender, EventArgs e)
        {
            if (CommunicationData.MainWindow.WelcomeScreenAllowed)
            {
                DoubleAnimation db = new DoubleAnimation(1, new Duration(new TimeSpan(0, 0, 1)));
                this.Content.BeginAnimation(UIElement.OpacityProperty, db);
            }
        }

        private void NavigationFrame_Navigated(object sender, NavigatingCancelEventArgs e)
        {
            var ta = new ThicknessAnimation();
            ta.Duration = TimeSpan.FromSeconds(1);
            QuadraticEase EasingFunction = new QuadraticEase();
            EasingFunction.EasingMode = EasingMode.EaseOut;
            ta.EasingFunction = EasingFunction;
            ta.DecelerationRatio = 0.7;
            ta.To = new Thickness(0, 0, 0, 0);
            if (e.NavigationMode == NavigationMode.New)
            {
                ta.From = new Thickness(500, 0, 0, 0);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                ta.From = new Thickness(0, 0, 500, 0);
            }

            var ta2 = new DoubleAnimation();
            ta2.To = 1;
            ta2.From = 0;
            QuadraticEase EasingFunction2 = new QuadraticEase();
            EasingFunction2.EasingMode = EasingMode.EaseOut;
            ta.EasingFunction = EasingFunction2;
            //(e.Content as Page).BeginAnimation(MarginProperty, ta);
            NavigationFrame.BeginAnimation(MarginProperty, ta);
            NavigationFrame.BeginAnimation(OpacityProperty, ta2);
        }
    }
}
