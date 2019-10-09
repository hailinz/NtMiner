﻿using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NTMiner.Views {
    internal class SafeNativeMethods {
        internal const int GWL_STYLE = -16;
        internal const int WS_VISIBLE = 0x10000000;
        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }

    public partial class ConsoleWindow : BlankWindow {
        public static readonly ConsoleWindow Instance = new ConsoleWindow();

        private ConsoleWindow() {
            this.Width = AppStatic.MainWindowWidth;
            this.Height = AppStatic.MainWindowHeight;
            InitializeComponent();
        }

        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        public void HideSplash() {
            Splash.Visibility = Visibility.Collapsed;
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        public void ReSizeConsoleWindow(int marginLeft, int marginTop, int marginBottom) {
            const int paddingLeft = 4;
            const int paddingRight = 5;
            int width = (int)this.ActualWidth - paddingLeft - paddingRight - marginLeft;
            if (width < 0) {
                width = 0;
            }
            int height = (int)this.ActualHeight - marginTop - marginBottom;

            IntPtr console = NTMinerConsole.Show();
            SafeNativeMethods.MoveWindow(console, paddingLeft + marginLeft, marginTop, width, height, true);
        }

        private void Window_SourceInitialized(object sender, EventArgs e) {
            IntPtr parent = new WindowInteropHelper(this).Handle;
            IntPtr console = NTMinerConsole.Show();
            SafeNativeMethods.SetParent(console, parent);
            SafeNativeMethods.SetWindowLong(console, SafeNativeMethods.GWL_STYLE, SafeNativeMethods.WS_VISIBLE);
        }
    }
}
