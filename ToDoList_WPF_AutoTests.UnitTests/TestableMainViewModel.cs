using ToDoList_WPF_AutoTests.Core;
using ToDoList_WPF_AutoTests.UI.ViewModels;

namespace ToDoList_WPF_AutoTests.UnitTests;

internal sealed class TestableMainViewModel : MainViewModel
{
    public TestableMainViewModel(ITodoRepository repository) : base(repository) { }

    protected override bool ConfirmRemove(int count) => true;
    protected override bool ConfirmComplete(int count) => true;
}
