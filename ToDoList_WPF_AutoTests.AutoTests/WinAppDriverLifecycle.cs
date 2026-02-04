using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToDoList_WPF_AutoTests.AutoTests;

[TestClass]
public class WinAppDriverLifecycle
{
    private const string WinAppDriverPath = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

    private static Process? _winAppDriverProcess;

    [AssemblyInitialize]
    public static void StartWinAppDriver(TestContext _)
    {
        if (!File.Exists(WinAppDriverPath))
        {
            Assert.Inconclusive($"WinAppDriver not found at {WinAppDriverPath}. Install or adjust path.");
            return;
        }

        _winAppDriverProcess = Process.Start(new ProcessStartInfo
        {
            FileName = WinAppDriverPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Minimized
        });

        if (_winAppDriverProcess != null && !_winAppDriverProcess.HasExited)
            Thread.Sleep(1500);
    }

    [AssemblyCleanup]
    public static void StopWinAppDriver()
    {
        if (_winAppDriverProcess == null || _winAppDriverProcess.HasExited)
            return;

        try
        {
            _winAppDriverProcess.Kill(entireProcessTree: true);
        }
        catch
        {
        }
        finally
        {
            _winAppDriverProcess = null;
        }
    }
}
