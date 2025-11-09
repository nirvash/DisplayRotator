using System;
using System.Runtime.InteropServices;

namespace DisplayRotator
{
    public static class Win32Api
    {
        // --- Display / Hotkey minimal P/Invoke ---
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int ChangeDisplaySettingsEx(string? lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int ChangeDisplaySettingsEx(string? lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, int dwflags, IntPtr lParam);

        // Hotkey API
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Bring window to foreground (used before showing context menu)
        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        // DEVMODE structure
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME   = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;           // 32 WCHAR = 64 bytes

            public short  dmSpecVersion;
            public short  dmDriverVersion;
            public short  dmSize;                 // ← 必ず Marshal.SizeOf<DEVMODE>() で代入
            public short  dmDriverExtra;
            public int    dmFields;

            // --- This section corresponds to the union with printer settings (for display, use the following two fields) ---
            public int    dmPositionX;            // Display position X (equivalent to POINTL.x)
            public int    dmPositionY;            // Display position Y (equivalent to POINTL.y)
            public int    dmDisplayOrientation;
            public int    dmDisplayFixedOutput;

            public short  dmColor;
            public short  dmDuplex;
            public short  dmYResolution;
            public short  dmTTOption;
            public short  dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;

            public short  dmLogPixels;
            public int    dmBitsPerPel;
            public int    dmPelsWidth;
            public int    dmPelsHeight;
            public int    dmDisplayFlags;
            public int    dmDisplayFrequency;

            public int    dmICMMethod;
            public int    dmICMIntent;
            public int    dmMediaType;
            public int    dmDitherType;
            public int    dmReserved1;
            public int    dmReserved2;
            public int    dmPanningWidth;
            public int    dmPanningHeight;
        }

        // --- Constants used by current code paths ---
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x00000001;
        public const int CDS_NORESET       = unchecked((int)0x10000000);
        public const int CDS_SET_PRIMARY   = 0x00000010;

        public const int DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
        public const int DISPLAY_DEVICE_PRIMARY_DEVICE      = 0x00000004;

        public const int DM_POSITION           = 0x00000020;
        public const int DM_DISPLAYORIENTATION = 0x00000080;
        public const int DM_PELSWIDTH          = 0x00080000;
        public const int DM_PELSHEIGHT         = 0x00100000;

        public const int DISP_CHANGE_SUCCESSFUL  = 0;
        public const int DISP_CHANGE_FAILED      = -1;

        // Window message (used in MainForm WndProc)
        public const int WM_HOTKEY = 0x0312;
    }
}
