using CommunityToolkit.Mvvm.ComponentModel;

namespace DemoApp.ViewModels;

public partial class RatingDemoViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _basicRating = 3;

    [ObservableProperty]
    private double _productRating = 4.5;

    [ObservableProperty]
    private double _feedbackRating;

    [ObservableProperty]
    private double _customRating = 2;

    [ObservableProperty]
    private double _readOnlyRating = 4.2;

    public RatingDemoViewModel()
    {
        Title = "Rating Demo";
    }

    partial void OnBasicRatingChanged(double value) => UpdateStatus($"Basic rating: {value} stars");
    partial void OnProductRatingChanged(double value) => UpdateStatus($"Product rating: {value} stars");
    partial void OnFeedbackRatingChanged(double value) => UpdateStatus($"Feedback: {value} stars");
    partial void OnCustomRatingChanged(double value) => UpdateStatus($"Custom rating: {value}/10");
}
