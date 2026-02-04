using System.Runtime.InteropServices;
using System.Globalization;

namespace ToDoList_WPF_AutoTests.AutoTests;

internal static class NativeKeyboard
{
    private const int KeyEventFKeyUp = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void BringWindowToForeground(IntPtr hWnd)
    {
        SetForegroundWindow(hWnd);
        Thread.Sleep(80);
    }

    public static IntPtr ParseWindowHandle(string handle)
    {
        if (string.IsNullOrWhiteSpace(handle))
            return IntPtr.Zero;
        var s = handle.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            s = s.Substring(2);
        var isHex = s.Length > 0 && s.Any(c => "0123456789".IndexOf(c) < 0);
        var value = isHex
            ? long.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            : long.Parse(s, CultureInfo.InvariantCulture);
        return new IntPtr(value);
    }

    public static void KeyDown(byte virtualKey)
    {
        keybd_event(virtualKey, 0, 0, 0);
    }

    public static void KeyUp(byte virtualKey)
    {
        keybd_event(virtualKey, 0, KeyEventFKeyUp, 0);
    }

    public static void PressKey(byte virtualKey)
    {
        keybd_event(virtualKey, 0, 0, 0);
        keybd_event(virtualKey, 0, KeyEventFKeyUp, 0);
    }

    private const byte VkReturn = 0x0D;
    private const byte VkEscape = 0x1B;
    private const byte VkTab = 0x09;
    private const byte VkControl = 0x11;

    public static void PressEnter() => PressKey(VkReturn);
    public static void PressEscape() => PressKey(VkEscape);

    public static void PressTabThenEnter()
    {
        PressKey(VkTab);
        Thread.Sleep(80);
        PressKey(VkReturn);
    }

    public static void ControlKeyDown() => KeyDown(VkControl);
    public static void ControlKeyUp() => KeyUp(VkControl);
}
