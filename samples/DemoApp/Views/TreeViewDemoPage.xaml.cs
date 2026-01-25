using DemoApp.ViewModels;

namespace DemoApp.Views;

public partial class TreeViewDemoPage : ContentPage
{
    public TreeViewDemoPage(TreeViewDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
