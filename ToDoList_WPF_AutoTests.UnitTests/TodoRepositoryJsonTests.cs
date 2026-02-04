using ToDoList_WPF_AutoTests.Core;
using ToDoList_WPF_AutoTests.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToDoList_WPF_AutoTests.UnitTests;

[TestClass]
public class TodoRepositoryJsonTests
{
    private string _tempPath = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"TodoRepoTest_{Guid.NewGuid():N}.json");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    [TestMethod]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var repo = new TodoRepositoryJson(_tempPath);
        var all = repo.GetAll();
        Assert.AreEqual(0, all.Count);
    }

    [TestMethod]
    public void Add_AddsItem_GetAllReturnsIt()
    {
        var repo = new TodoRepositoryJson(_tempPath);
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        repo.Add(item);
        var all = repo.GetAll();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual(item.Id, all[0].Id);
        Assert.AreEqual("Test", all[0].Title);
    }

    [TestMethod]
    public void Remove_RemovesItem()
    {
        var repo = new TodoRepositoryJson(_tempPath);
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "To Remove",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        repo.Add(item);
        repo.Remove(item.Id);
        Assert.AreEqual(0, repo.GetAll().Count);
    }

    [TestMethod]
    public void Update_UpdatesItem()
    {
        var repo = new TodoRepositoryJson(_tempPath);
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        repo.Add(item);
        item.Title = "Updated";
        item.IsCompleted = true;
        repo.Update(item);
        var all = repo.GetAll();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("Updated", all[0].Title);
        Assert.IsTrue(all[0].IsCompleted);
    }

    [TestMethod]
    public void Persistence_AfterSave_LoadsFromFile()
    {
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "Persistence",
            IsCompleted = true,
            CreatedAt = DateTime.UtcNow
        };
        var repo = new TodoRepositoryJson(_tempPath);
        repo.Add(item);
        // Reopen from file
        var repo2 = new TodoRepositoryJson(_tempPath);
        var all = repo2.GetAll();
        Assert.AreEqual(1, all.Count);
        Assert.AreEqual(item.Id, all[0].Id);
        Assert.AreEqual("Persistence", all[0].Title);
        Assert.IsTrue(all[0].IsCompleted);
    }
}
