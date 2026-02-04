using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToDoList_WPF_AutoTests.AutoTests;

[TestClass]
public class TodoListUiTests
{
    private const string WinAppDriverUrl = "http://127.0.0.1:4723";
    private RemoteWebDriver? _driver;

    [TestInitialize]
    public void Setup()
    {
        var exePath = AppPathHelper.GetUiExePath();
        if (!File.Exists(exePath))
            throw new InvalidOperationException(
                $"UI app not found at {exePath}. Build ToDoList_WPF_AutoTests.UI first.");

        var dataFile = GetDataFilePath();
        if (File.Exists(dataFile))
            File.Delete(dataFile);

        var caps = new DesiredCapabilities();
        caps.SetCapability("app", exePath);
        caps.SetCapability("platformName", "Windows");
        caps.SetCapability("deviceName", "WindowsPC");

        _driver = new RemoteWebDriver(new Uri(WinAppDriverUrl), caps);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Assert.IsNotNull(_driver);
    }

    [TestCleanup]
    public void TestsCleanup()
    {
        _driver?.Dispose();
        _driver = null;
    }

    private static By ByAutomationId(string automationId) =>
        By.Name(automationId);

    private static string GetDataFilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ToDoList",
            "todos.json");
    }

    private static IWebElement? WaitForElement(RemoteWebDriver driver, By by, TimeSpan timeout)
    {
        var stopAt = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < stopAt)
        {
            var found = driver.FindElements(by);
            if (found.Count > 0)
                return found[0];
            Thread.Sleep(200);
        }
        return null;
    }

    [TestMethod]
    public void AddTodo_AddsItemToList()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        var newItemBox = driver.FindElement(ByAutomationId("NewItemTextBox"));
        newItemBox.Clear();
        newItemBox.SendKeys("Auto test task");
        driver.FindElement(ByAutomationId("AddButton")).Click();

        var item = WaitForElement(driver, By.Name("Auto test task"), TimeSpan.FromSeconds(5));
        Assert.IsNotNull(item, "New item should appear in the list");
    }

    [TestMethod]
    public void RemoveTodo_RemovesSelectedItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        var newItemBox = driver.FindElement(ByAutomationId("NewItemTextBox"));
        newItemBox.Clear();
        newItemBox.SendKeys("To be removed");
        driver.FindElement(ByAutomationId("AddButton")).Click();

        var toRemove = WaitForElement(driver, By.Name("To be removed"), TimeSpan.FromSeconds(5));
        Assert.IsNotNull(toRemove);
        toRemove!.Click();

        driver.FindElement(ByAutomationId("RemoveButton")).Click();

        var stillThere = WaitForElement(driver, By.Name("To be removed"), TimeSpan.FromSeconds(5));
        Assert.IsNull(stillThere, "Item should be removed from the list");
    }
}
