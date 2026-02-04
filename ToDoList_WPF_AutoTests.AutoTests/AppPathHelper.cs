using System.Reflection;

namespace ToDoList_WPF_AutoTests.AutoTests;

public static class AppPathHelper
{
    public static string GetUiExePath()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? AppDomain.CurrentDomain.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
        var exePath = Path.Combine(
            solutionRoot,
            "ToDoList_WPF_AutoTests.UI",
            "bin",
            "Debug",
            "net8.0-windows",
            "ToDoList_WPF_AutoTests.UI.exe");
        return exePath;
    }
}
