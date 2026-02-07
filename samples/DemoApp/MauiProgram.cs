using DemoApp.ViewModels;
using DemoApp.Views;
using Microsoft.Extensions.Logging;

namespace DemoApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AccordionDemoViewModel>();
        builder.Services.AddTransient<BindingNavigatorDemoViewModel>();
        builder.Services.AddTransient<BreadcrumbDemoViewModel>();
        builder.Services.AddTransient<CalendarDemoViewModel>();
        builder.Services.AddTransient<ComboBoxDemoViewModel>();
        builder.Services.AddTransient<DataGridDemoViewModel>();
        builder.Services.AddTransient<MultiSelectDemoViewModel>();
        builder.Services.AddTransient<NumericUpDownDemoViewModel>();
        builder.Services.AddTransient<PropertyGridDemoViewModel>();
        builder.Services.AddTransient<RangeSliderDemoViewModel>();
        builder.Services.AddTransient<RatingDemoViewModel>();
        builder.Services.AddTransient<RichTextEditorDemoViewModel>();
        builder.Services.AddTransient<TokenEntryDemoViewModel>();
        builder.Services.AddTransient<TreeViewDemoViewModel>();
        builder.Services.AddTransient<WizardDemoViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AccordionDemoPage>();
        builder.Services.AddTransient<BindingNavigatorDemoPage>();
        builder.Services.AddTransient<BreadcrumbDemoPage>();
        builder.Services.AddTransient<CalendarDemoPage>();
        builder.Services.AddTransient<ComboBoxDemoPage>();
        builder.Services.AddTransient<DataGridDemoPage>();
        builder.Services.AddTransient<MultiSelectDemoPage>();
        builder.Services.AddTransient<NumericUpDownDemoPage>();
        builder.Services.AddTransient<PropertyGridDemoPage>();
        builder.Services.AddTransient<RangeSliderDemoPage>();
        builder.Services.AddTransient<RatingDemoPage>();
        builder.Services.AddTransient<RichTextEditorDemoPage>();
        builder.Services.AddTransient<TokenEntryDemoPage>();
        builder.Services.AddTransient<TreeViewDemoPage>();
        builder.Services.AddTransient<WizardDemoPage>();

        return builder.Build();
    }
}
