using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DemoApp.Models;

namespace DemoApp.ViewModels;

public partial class TreeViewDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<FolderItem> _folders = [];

    [ObservableProperty]
    private FolderItem? _selectedItem;

    public TreeViewDemoViewModel()
    {
        Title = "TreeView Demo";
        Folders = new ObservableCollection<FolderItem>(SampleData.Folders);
    }

    partial void OnSelectedItemChanged(FolderItem? value)
    {
        if (value is not null)
            UpdateStatus($"Selected: {value.Name}");
    }
}
