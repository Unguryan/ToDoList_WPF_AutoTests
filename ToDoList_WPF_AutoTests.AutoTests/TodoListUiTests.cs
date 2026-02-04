using System.Globalization;
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

    private void OpenAddDialog(RemoteWebDriver driver)
    {
        driver.FindElement(ByAutomationId("AddButton")).Click();
        var handles = driver.WindowHandles;
        if (handles.Count >= 2)
            driver.SwitchTo().Window(handles[handles.Count - 1]);
    }

    private void CloseAddDialog(RemoteWebDriver driver, bool ok)
    {
        driver.FindElement(By.Name(ok ? "OkButton" : "CancelButton")).Click();
        var handles = driver.WindowHandles;
        if (handles.Count >= 2)
            driver.SwitchTo().Window(handles[0]);
    }

    private void SetDeadlineInAddDialog(RemoteWebDriver driver, DateTime date)
    {
        var datePicker = WaitForElement(driver, By.Name("DeadlineDatePicker"), TimeSpan.FromSeconds(2));
        if (datePicker == null) return;
        datePicker.Click();
        datePicker.SendKeys(Keys.Control + "a");
        var dateString = date.ToString("d", CultureInfo.CurrentCulture);
        datePicker.SendKeys(dateString);
    }

    private void OpenAddDialogAndAddItem(RemoteWebDriver driver, string title, string? description = null, DateTime? deadline = null)
    {
        OpenAddDialog(driver);
        var titleBox = WaitForElement(driver, By.Name("TitleTextBox"), TimeSpan.FromSeconds(3));
        Assert.IsNotNull(titleBox);
        titleBox!.Clear();
        titleBox.SendKeys(title);
        if (deadline.HasValue)
            SetDeadlineInAddDialog(driver, deadline.Value);
        if (!string.IsNullOrEmpty(description))
        {
            var descBox = driver.FindElement(By.Name("DescriptionTextBox"));
            descBox.Clear();
            descBox.SendKeys(description);
        }
        CloseAddDialog(driver, ok: true);
    }

    private static readonly string[] YesButtonNames = { "Yes", "Так", "Да" };
    private static readonly string[] NoButtonNames = { "No", "Ні", "Нет" };

    private void ConfirmMessageBox(RemoteWebDriver driver, bool yes)
    {
        Thread.Sleep(500); 
        var handles = driver.WindowHandles;
        var names = yes ? YesButtonNames : NoButtonNames;
        IWebElement? btn = null;

        if (handles.Count >= 2)
        {
            try
            {
                driver.SwitchTo().Window(handles[handles.Count - 1]);
                foreach (var name in names)
                {
                    btn = WaitForElement(driver, By.Name(name), TimeSpan.FromMilliseconds(500));
                    if (btn != null) break;
                }
                if (btn != null)
                {
                    btn.Click();
                    driver.SwitchTo().Window(handles[0]);
                    Thread.Sleep(200);
                    return;
                }
                driver.SwitchTo().Window(handles[0]);
            }
            catch
            {
                try { driver.SwitchTo().Window(handles[0]); } catch { /* ignore */ }
            }
        }

        if (yes)
            NativeKeyboard.PressEnter();
        else
            NativeKeyboard.PressTabThenEnter();
        Thread.Sleep(200);
    }

    private void SelectMultipleItems(RemoteWebDriver driver, params string[] itemTitles)
    {
        foreach (var title in itemTitles)
        {
            var checkBoxName = $"Select {title}";
            var el = WaitForElement(driver, By.Name(checkBoxName), TimeSpan.FromSeconds(3));
            Assert.IsNotNull(el, $"Checkbox '{checkBoxName}' not found.");
            el!.Click();
            Thread.Sleep(100);
        }
    }

    [TestMethod]
    public void AddTodo_AddsItemToList()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "Auto test task");

        var item = WaitForElement(driver, By.Name("Auto test task"), TimeSpan.FromSeconds(5));
        Assert.IsNotNull(item, "New item should appear in the list");
    }

    [TestMethod]
    public void RemoveTodo_RemovesSelectedItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "To be removed");

        SelectMultipleItems(driver, "To be removed");
        driver.FindElement(ByAutomationId("RemoveButton")).Click();
        ConfirmMessageBox(driver, yes: true);

        var stillThere = WaitForElement(driver, By.Name("To be removed"), TimeSpan.FromSeconds(3));
        Assert.IsNull(stillThere, "Item should be removed from the list");
    }

    [TestMethod]
    public void AddDialog_Cancel_DoesNotAddItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialog(driver);
        var titleBox = WaitForElement(driver, By.Name("TitleTextBox"), TimeSpan.FromSeconds(3));
        Assert.IsNotNull(titleBox);
        titleBox!.SendKeys("Should not appear");
        CloseAddDialog(driver, ok: false);

        var item = WaitForElement(driver, By.Name("Should not appear"), TimeSpan.FromSeconds(2));
        Assert.IsNull(item, "Cancel should not add an item");
    }

    [TestMethod]
    public void Remove_ConfirmNo_KeepsItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "Keep me");
        SelectMultipleItems(driver, "Keep me");
        driver.FindElement(ByAutomationId("RemoveButton")).Click();
        ConfirmMessageBox(driver, yes: false);

        var stillThere = WaitForElement(driver, By.Name("Keep me"), TimeSpan.FromSeconds(3));
        Assert.IsNotNull(stillThere, "Item should remain when user clicks No");
    }

    [TestMethod]
    public void Complete_SingleItem_WithConfirmation()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "To complete");
        SelectMultipleItems(driver, "To complete");
        driver.FindElement(ByAutomationId("CompleteButton")).Click();
        ConfirmMessageBox(driver, yes: true);

        var stillThere = WaitForElement(driver, By.Name("To complete"), TimeSpan.FromSeconds(3));
        Assert.IsNotNull(stillThere, "Item should remain in list after complete");
    }

    [TestMethod]
    public void AddTodo_WithDescription_AddsItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "Task with notes", "My description text");
        var item = WaitForElement(driver, By.Name("Task with notes"), TimeSpan.FromSeconds(5));
        Assert.IsNotNull(item, "Item with description should appear in the list");
    }

    [TestMethod]
    public void MultiSelect_Remove_TwoItems()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "First");
        OpenAddDialogAndAddItem(driver, "Second");
        SelectMultipleItems(driver, "First", "Second");
        driver.FindElement(ByAutomationId("RemoveButton")).Click();
        ConfirmMessageBox(driver, yes: true);

        Assert.IsNull(WaitForElement(driver, By.Name("First"), TimeSpan.FromSeconds(2)), "First should be removed");
        Assert.IsNull(WaitForElement(driver, By.Name("Second"), TimeSpan.FromSeconds(2)), "Second should be removed");
    }

    [TestMethod]
    public void MultiSelect_Complete_TwoItems()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        OpenAddDialogAndAddItem(driver, "Complete A");
        OpenAddDialogAndAddItem(driver, "Complete B");
        SelectMultipleItems(driver, "Complete A", "Complete B");
        driver.FindElement(ByAutomationId("CompleteButton")).Click();
        ConfirmMessageBox(driver, yes: true);

        Assert.IsNotNull(WaitForElement(driver, By.Name("Complete A"), TimeSpan.FromSeconds(2)), "Complete A should remain");
        Assert.IsNotNull(WaitForElement(driver, By.Name("Complete B"), TimeSpan.FromSeconds(2)), "Complete B should remain");
    }

    [TestMethod]
    public void AddTodo_WithNextDayDeadline_AddsItem()
    {
        var driver = _driver ?? throw new InvalidOperationException("Driver not initialized.");
        var nextDay = DateTime.Today.AddDays(1);
        OpenAddDialogAndAddItem(driver, "Next day task", deadline: nextDay);

        var item = WaitForElement(driver, By.Name("Next day task"), TimeSpan.FromSeconds(5));
        Assert.IsNotNull(item, "Item with next-day deadline should appear in the list");
    }
}
