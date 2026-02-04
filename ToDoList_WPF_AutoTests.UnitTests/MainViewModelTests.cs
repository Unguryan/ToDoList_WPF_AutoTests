using ToDoList_WPF_AutoTests.Core.Models;
using ToDoList_WPF_AutoTests.UI.ViewModels;
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
    public void Add_AddsItemToCollection()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new MainViewModel(repo);
        vm.NewItemTitle = "New task";
        vm.AddCommand.Execute(null);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("New task", vm.Items[0].Title);
        Assert.IsFalse(vm.Items[0].IsCompleted);
    }

    [TestMethod]
    public void Add_ClearsNewItemTitle()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new MainViewModel(repo);
        vm.NewItemTitle = "Task";
        vm.AddCommand.Execute(null);
        Assert.AreEqual(string.Empty, vm.NewItemTitle);
    }

    [TestMethod]
    public void Remove_RemovesSelectedItem()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new MainViewModel(repo);
        vm.NewItemTitle = "One";
        vm.AddCommand.Execute(null);
        vm.NewItemTitle = "Two";
        vm.AddCommand.Execute(null);
        vm.SelectedItem = vm.Items[0];
        vm.RemoveCommand.Execute(null);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("Two", vm.Items[0].Title);
    }

    [TestMethod]
    public void ToggleComplete_TogglesIsCompleted()
    {
        var items = new List<TodoItem>();
        var repo = CreateRepo(items);
        var vm = new MainViewModel(repo);
        vm.NewItemTitle = "Task";
        vm.AddCommand.Execute(null);
        var item = vm.Items[0];
        vm.SelectedItem = item;
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
        var vm = new MainViewModel(repo);
        Assert.AreEqual(1, vm.Items.Count);
        Assert.AreEqual("Existing", vm.Items[0].Title);
    }
}
