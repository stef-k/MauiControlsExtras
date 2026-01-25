using DemoApp.ViewModels;
using MauiControlsExtras.Controls;

namespace DemoApp.Views;

public partial class BreadcrumbDemoPage : ContentPage
{
    private readonly BreadcrumbDemoViewModel _viewModel;

    public BreadcrumbDemoPage(BreadcrumbDemoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Initialize breadcrumb items after page is loaded
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        // Add items to all breadcrumbs
        InitializeBreadcrumb(MainBreadcrumb);
        InitializeBreadcrumb(ArrowBreadcrumb);
        InitializeBreadcrumb(StyledBreadcrumb);
    }

    private static void InitializeBreadcrumb(Breadcrumb breadcrumb)
    {
        breadcrumb.Items.Clear();
        breadcrumb.Items.Add(new BreadcrumbItem { Text = "Home" });
        breadcrumb.Items.Add(new BreadcrumbItem { Text = "Products" });
        breadcrumb.Items.Add(new BreadcrumbItem { Text = "Electronics" });
        breadcrumb.Items.Add(new BreadcrumbItem { Text = "Phones" });
    }

    private void OnBreadcrumbItemClicked(object? sender, BreadcrumbItemClickedEventArgs e)
    {
        _viewModel.NavigateToCommand.Execute(e.Item.Text);

        // Truncate breadcrumb to clicked item
        if (sender is Breadcrumb breadcrumb)
        {
            while (breadcrumb.Items.Count > e.Index + 1)
            {
                breadcrumb.Items.RemoveAt(breadcrumb.Items.Count - 1);
            }
        }
    }
}
