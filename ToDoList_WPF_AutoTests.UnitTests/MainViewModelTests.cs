using ToDoList_WPF_AutoTests.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ToDoList_WPF_AutoTests.Core;

namespace ToDoList_WPF_AutoTests.UnitTests;

[TestClass]
public class MainViewModelTests
{
    private static ITodoRepository CreateRepo(List<TodoItem> items)
    {
        var repo = Substitute.For<ITodoRepository>();
        repo.GetAll().Returns(items);
        repo.When(r => r.Add(Arg.Any<TodoItem>()))
            .Do(ci => items.Add(ci.Arg<TodoItem>()));
        repo.When(r => r.Remove(Arg.Any<Guid>()))
            .Do(ci => items.RemoveAll(x => x.Id == ci.Arg<Guid>()));
        repo.When(r => r.Update(Arg.Any<TodoItem>()))
            .Do(ci =>
            {
                var item = ci.Arg<TodoItem>();
                var index = items.FindIndex(x => x.Id == item.Id);
                if (index >= 0)
                    items[index] = item;
            });
        return repo;
    }

    [TestMethod]
    public void AddItem_AddsToCollection()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new TestableMainViewModel(repo);
        var item = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = "New task",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
        vm.AddItem(item);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("New task", vm.Items[0].Title);
        Assert.IsFalse(vm.Items[0].IsCompleted);
    }

    [TestMethod]
    public void Remove_RemovesSelectedItems()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new TestableMainViewModel(repo);
        vm.AddItem(new TodoItem { Id = Guid.NewGuid(), Title = "One", CreatedAt = DateTime.UtcNow });
        vm.AddItem(new TodoItem { Id = Guid.NewGuid(), Title = "Two", CreatedAt = DateTime.UtcNow });
        vm.SyncSelectedItems(new[] { vm.Items[0] });
        vm.RemoveCommand.Execute(null);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("Two", vm.Items[0].Title);
    }

    [TestMethod]
    public void ToggleComplete_TogglesIsCompleted()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new TestableMainViewModel(repo);
        vm.AddItem(new TodoItem { Id = Guid.NewGuid(), Title = "Task", CreatedAt = DateTime.UtcNow });
        var item = vm.Items[0];
        vm.SyncSelectedItems(new[] { item });
        Assert.IsFalse(item.IsCompleted);
        vm.ToggleCompleteCommand.Execute(null);
        Assert.IsTrue(item.IsCompleted);
        vm.ToggleCompleteCommand.Execute(null);
        Assert.IsFalse(item.IsCompleted);
    }

    [TestMethod]
    public void LoadItems_PopulatesFromRepository()
    {
        var items = new List<TodoItem>
        {
            new TodoItem
            {
                Id = Guid.NewGuid(),
                Title = "Existing",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };
        var repo = CreateRepo(items);
        var vm = new TestableMainViewModel(repo);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("Existing", vm.Items[0].Title);
    }
}
