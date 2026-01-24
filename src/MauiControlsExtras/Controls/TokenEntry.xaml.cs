using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A text entry control that converts typed values into removable tokens/chips.
/// </summary>
public partial class TokenEntry : TextStyledControlBase, IValidatable
{
    #region Fields

    private Entry? _inputEntry;
    private bool _isUpdatingTokens;
    private readonly List<string> _validationErrors = new();
    private string _previousText = string.Empty;

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty TokensProperty = BindableProperty.Create(
        nameof(Tokens),
        typeof(ObservableCollection<string>),
        typeof(TokenEntry),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnTokensChanged);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(TokenEntry),
        default(string),
        BindingMode.TwoWay);

    public static readonly BindableProperty DelimiterProperty = BindableProperty.Create(
        nameof(Delimiter),
        typeof(char),
        typeof(TokenEntry),
        ',');

    public static readonly BindableProperty CreateTokenOnEnterProperty = BindableProperty.Create(
        nameof(CreateTokenOnEnter),
        typeof(bool),
        typeof(TokenEntry),
        true);

    public static readonly BindableProperty CreateTokenOnFocusLostProperty = BindableProperty.Create(
        nameof(CreateTokenOnFocusLost),
        typeof(bool),
        typeof(TokenEntry),
        true);

    public static readonly BindableProperty MaxTokensProperty = BindableProperty.Create(
        nameof(MaxTokens),
        typeof(int?),
        typeof(TokenEntry),
        default(int?));

    public static readonly BindableProperty MaxTokenLengthProperty = BindableProperty.Create(
        nameof(MaxTokenLength),
        typeof(int?),
        typeof(TokenEntry),
        default(int?));

    public static readonly BindableProperty AllowDuplicatesProperty = BindableProperty.Create(
        nameof(AllowDuplicates),
        typeof(bool),
        typeof(TokenEntry),
        false);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(TokenEntry),
        "Add item...");

    public static readonly BindableProperty SuggestionsSourceProperty = BindableProperty.Create(
        nameof(SuggestionsSource),
        typeof(IEnumerable),
        typeof(TokenEntry),
        default(IEnumerable));

    public static readonly BindableProperty FilteredSuggestionsProperty = BindableProperty.Create(
        nameof(FilteredSuggestions),
        typeof(ObservableCollection<string>),
        typeof(TokenEntry),
        default(ObservableCollection<string>));

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(TokenEntry),
        false);

    public static readonly BindableProperty MinTokensProperty = BindableProperty.Create(
        nameof(MinTokens),
        typeof(int),
        typeof(TokenEntry),
        0);

    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(TokenEntry),
        "At least one item is required.");

    #endregion

    #region Command Properties

    public static readonly BindableProperty TokenAddedCommandProperty = BindableProperty.Create(
        nameof(TokenAddedCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty TokenRemovedCommandProperty = BindableProperty.Create(
        nameof(TokenRemovedCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty TokensChangedCommandProperty = BindableProperty.Create(
        nameof(TokensChangedCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty InvalidTokenAttemptedCommandProperty = BindableProperty.Create(
        nameof(InvalidTokenAttemptedCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the collection of tokens.
    /// </summary>
    public ObservableCollection<string>? Tokens
    {
        get => (ObservableCollection<string>?)GetValue(TokensProperty);
        set => SetValue(TokensProperty, value);
    }

    /// <summary>
    /// Gets or sets the current text being typed.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the delimiter character that creates a token.
    /// </summary>
    public char Delimiter
    {
        get => (char)GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether Enter key creates a token.
    /// </summary>
    public bool CreateTokenOnEnter
    {
        get => (bool)GetValue(CreateTokenOnEnterProperty);
        set => SetValue(CreateTokenOnEnterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether losing focus creates a token.
    /// </summary>
    public bool CreateTokenOnFocusLost
    {
        get => (bool)GetValue(CreateTokenOnFocusLostProperty);
        set => SetValue(CreateTokenOnFocusLostProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of tokens.
    /// </summary>
    public int? MaxTokens
    {
        get => (int?)GetValue(MaxTokensProperty);
        set => SetValue(MaxTokensProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum length of each token.
    /// </summary>
    public int? MaxTokenLength
    {
        get => (int?)GetValue(MaxTokenLengthProperty);
        set => SetValue(MaxTokenLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets whether duplicate tokens are allowed.
    /// </summary>
    public bool AllowDuplicates
    {
        get => (bool)GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the source for autocomplete suggestions.
    /// </summary>
    public IEnumerable? SuggestionsSource
    {
        get => (IEnumerable?)GetValue(SuggestionsSourceProperty);
        set => SetValue(SuggestionsSourceProperty, value);
    }

    /// <summary>
    /// Gets the filtered suggestions.
    /// </summary>
    public ObservableCollection<string> FilteredSuggestions
    {
        get => (ObservableCollection<string>)GetValue(FilteredSuggestionsProperty);
        private set => SetValue(FilteredSuggestionsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether at least one token is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum number of tokens required.
    /// </summary>
    public int MinTokens
    {
        get => (int)GetValue(MinTokensProperty);
        set => SetValue(MinTokensProperty, value);
    }

    /// <summary>
    /// Gets or sets the required error message.
    /// </summary>
    public string RequiredErrorMessage
    {
        get => (string)GetValue(RequiredErrorMessageProperty);
        set => SetValue(RequiredErrorMessageProperty, value);
    }

    /// <summary>
    /// Gets or sets a custom validation function for tokens.
    /// </summary>
    public Func<string, bool>? ValidationFunc { get; set; }

    /// <summary>
    /// Gets the token count.
    /// </summary>
    public int TokenCount => Tokens?.Count ?? 0;

    /// <summary>
    /// Gets whether max tokens limit is reached.
    /// </summary>
    public bool IsMaxReached => MaxTokens.HasValue && TokenCount >= MaxTokens.Value;

    /// <summary>
    /// Gets the current border color.
    /// </summary>
    public Color CurrentBorderColor
    {
        get
        {
            if (!IsValid)
                return EffectiveErrorBorderColor;
            if (_inputEntry?.IsFocused == true)
                return EffectiveFocusBorderColor;
            return EffectiveBorderColor;
        }
    }

    #endregion

    #region Command Properties (CLR)

    public ICommand? TokenAddedCommand
    {
        get => (ICommand?)GetValue(TokenAddedCommandProperty);
        set => SetValue(TokenAddedCommandProperty, value);
    }

    public ICommand? TokenRemovedCommand
    {
        get => (ICommand?)GetValue(TokenRemovedCommandProperty);
        set => SetValue(TokenRemovedCommandProperty, value);
    }

    public ICommand? TokensChangedCommand
    {
        get => (ICommand?)GetValue(TokensChangedCommandProperty);
        set => SetValue(TokensChangedCommandProperty, value);
    }

    public ICommand? InvalidTokenAttemptedCommand
    {
        get => (ICommand?)GetValue(InvalidTokenAttemptedCommandProperty);
        set => SetValue(InvalidTokenAttemptedCommandProperty, value);
    }

    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    #endregion

    #region IValidatable

    public bool IsValid => _validationErrors.Count == 0;

    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    public ValidationResult Validate()
    {
        var wasValid = IsValid;
        _validationErrors.Clear();

        var count = TokenCount;

        if (IsRequired && count == 0)
        {
            _validationErrors.Add(RequiredErrorMessage);
        }

        if (MinTokens > 0 && count < MinTokens)
        {
            _validationErrors.Add($"At least {MinTokens} item(s) required.");
        }

        if (MaxTokens.HasValue && count > MaxTokens.Value)
        {
            _validationErrors.Add($"Maximum {MaxTokens.Value} item(s) allowed.");
        }

        var isNowValid = IsValid;

        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationErrors));
        OnPropertyChanged(nameof(CurrentBorderColor));

        var result = _validationErrors.Count == 0
            ? ValidationResult.Success
            : ValidationResult.Failure(_validationErrors);

        if (ValidateCommand?.CanExecute(result) == true)
        {
            ValidateCommand.Execute(result);
        }

        return result;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a token is added.
    /// </summary>
    public event EventHandler<string>? TokenAdded;

    /// <summary>
    /// Occurs when a token is removed.
    /// </summary>
    public event EventHandler<string>? TokenRemoved;

    /// <summary>
    /// Occurs when the tokens collection changes.
    /// </summary>
    public event EventHandler<IList<string>>? TokensChanged;

    /// <summary>
    /// Occurs when token validation fails.
    /// </summary>
    public event EventHandler<string>? InvalidTokenAttempted;

    #endregion

    #region Constructor

    public TokenEntry()
    {
        InitializeComponent();
        FilteredSuggestions = new ObservableCollection<string>();
        EnsureTokensCollection();
        BuildLayout();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnTokensChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TokenEntry control)
        {
            // Unsubscribe from old
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnTokensCollectionChanged;
            }

            // Subscribe to new
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.OnTokensCollectionChanged;
            }

            control.RebuildTokenChips();
            control.OnPropertyChanged(nameof(TokenCount));
            control.OnPropertyChanged(nameof(IsMaxReached));
        }
    }

    private void OnTokensCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isUpdatingTokens) return;

        RebuildTokenChips();
        OnPropertyChanged(nameof(TokenCount));
        OnPropertyChanged(nameof(IsMaxReached));
        RaiseTokensChanged();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a token programmatically.
    /// </summary>
    public bool AddToken(string token)
    {
        return TryAddToken(token);
    }

    /// <summary>
    /// Removes a token programmatically.
    /// </summary>
    public bool RemoveToken(string token)
    {
        if (Tokens == null || !Tokens.Contains(token))
            return false;

        _isUpdatingTokens = true;
        try
        {
            Tokens.Remove(token);
            RebuildTokenChips();
            OnPropertyChanged(nameof(TokenCount));
            OnPropertyChanged(nameof(IsMaxReached));
            RaiseTokenRemoved(token);
            RaiseTokensChanged();
            return true;
        }
        finally
        {
            _isUpdatingTokens = false;
        }
    }

    /// <summary>
    /// Clears all tokens.
    /// </summary>
    public void Clear()
    {
        if (Tokens == null || Tokens.Count == 0) return;

        _isUpdatingTokens = true;
        try
        {
            var tokens = Tokens.ToList();
            Tokens.Clear();
            RebuildTokenChips();
            OnPropertyChanged(nameof(TokenCount));
            OnPropertyChanged(nameof(IsMaxReached));

            foreach (var token in tokens)
            {
                RaiseTokenRemoved(token);
            }
            RaiseTokensChanged();
        }
        finally
        {
            _isUpdatingTokens = false;
        }
    }

    /// <summary>
    /// Focuses the input entry.
    /// </summary>
    public new void Focus()
    {
        _inputEntry?.Focus();
    }

    #endregion

    #region Private Methods

    private void EnsureTokensCollection()
    {
        if (Tokens == null)
        {
            Tokens = new ObservableCollection<string>();
        }
    }

    private void BuildLayout()
    {
        RebuildTokenChips();
    }

    private void RebuildTokenChips()
    {
        tokensContainer.Children.Clear();

        // Add token chips
        if (Tokens != null)
        {
            foreach (var token in Tokens)
            {
                var chip = CreateTokenChip(token);
                tokensContainer.Children.Add(chip);
            }
        }

        // Add input entry at end
        CreateInputEntry();
        tokensContainer.Children.Add(_inputEntry!);
    }

    private View CreateTokenChip(string token)
    {
        var chipBorder = new Border
        {
            BackgroundColor = EffectiveAccentColor.WithAlpha(0.15f),
            StrokeThickness = 0,
            Padding = new Thickness(10, 6),
            Margin = new Thickness(2),
            StrokeShape = new RoundRectangle { CornerRadius = 14 }
        };

        var chipGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 6
        };

        var textLabel = new Label
        {
            Text = token,
            FontSize = 13,
            TextColor = EffectiveAccentColor,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(textLabel, 0);
        chipGrid.Add(textLabel);

        var removeLabel = new Label
        {
            Text = "✕",
            FontSize = 11,
            TextColor = EffectiveAccentColor.WithAlpha(0.7f),
            VerticalOptions = LayoutOptions.Center
        };
        var removeTap = new TapGestureRecognizer();
        removeTap.Tapped += (s, e) => RemoveToken(token);
        removeLabel.GestureRecognizers.Add(removeTap);
        Grid.SetColumn(removeLabel, 1);
        chipGrid.Add(removeLabel);

        chipBorder.Content = chipGrid;
        return chipBorder;
    }

    private void CreateInputEntry()
    {
        _inputEntry = new Entry
        {
            Placeholder = TokenCount == 0 ? Placeholder : string.Empty,
            PlaceholderColor = EffectivePlaceholderColor,
            TextColor = EffectiveTextColor,
            FontSize = EffectiveFontSize,
            BackgroundColor = Colors.Transparent,
            MinimumWidthRequest = 100,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0)
        };

        _inputEntry.TextChanged += OnInputTextChanged;
        _inputEntry.Completed += OnInputCompleted;
        _inputEntry.Focused += OnInputFocused;
        _inputEntry.Unfocused += OnInputUnfocused;
    }

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        var newText = e.NewTextValue ?? string.Empty;
        Text = newText;

        // Check for delimiter
        if (newText.Contains(Delimiter))
        {
            var parts = newText.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                TryAddToken(part.Trim());
            }
            _inputEntry!.Text = string.Empty;
            HideSuggestions();
            return;
        }

        // Handle backspace to remove last token
        if (string.IsNullOrEmpty(newText) && !string.IsNullOrEmpty(_previousText) && _previousText.Length == 1)
        {
            // Backspace was pressed when input was almost empty - but we can't detect this reliably
            // So we'll only remove on empty input + another backspace which we can't detect
        }

        _previousText = newText;

        // Update suggestions
        UpdateSuggestions(newText);
    }

    private void OnInputCompleted(object? sender, EventArgs e)
    {
        if (CreateTokenOnEnter && !string.IsNullOrWhiteSpace(_inputEntry?.Text))
        {
            if (TryAddToken(_inputEntry.Text.Trim()))
            {
                _inputEntry.Text = string.Empty;
            }
        }
        HideSuggestions();
    }

    private void OnInputFocused(object? sender, FocusEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentBorderColor));
        UpdateSuggestions(_inputEntry?.Text ?? string.Empty);
    }

    private void OnInputUnfocused(object? sender, FocusEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentBorderColor));

        if (CreateTokenOnFocusLost && !string.IsNullOrWhiteSpace(_inputEntry?.Text))
        {
            if (TryAddToken(_inputEntry.Text.Trim()))
            {
                _inputEntry.Text = string.Empty;
            }
        }

        // Delay hiding to allow suggestion tap
        Task.Delay(200).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(HideSuggestions);
        });

        Validate();
    }

    private void OnSuggestionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is string suggestion)
        {
            if (TryAddToken(suggestion))
            {
                _inputEntry!.Text = string.Empty;
            }
            HideSuggestions();
            _inputEntry?.Focus();
        }
    }

    private bool TryAddToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        token = token.Trim();

        // Check max length
        if (MaxTokenLength.HasValue && token.Length > MaxTokenLength.Value)
        {
            token = token[..MaxTokenLength.Value];
        }

        // Check max tokens
        if (IsMaxReached)
        {
            RaiseInvalidTokenAttempted(token);
            return false;
        }

        // Check duplicates
        if (!AllowDuplicates && Tokens?.Contains(token, StringComparer.OrdinalIgnoreCase) == true)
        {
            RaiseInvalidTokenAttempted(token);
            return false;
        }

        // Custom validation
        if (ValidationFunc != null && !ValidationFunc(token))
        {
            RaiseInvalidTokenAttempted(token);
            return false;
        }

        EnsureTokensCollection();

        _isUpdatingTokens = true;
        try
        {
            Tokens!.Add(token);
            RebuildTokenChips();
            OnPropertyChanged(nameof(TokenCount));
            OnPropertyChanged(nameof(IsMaxReached));
            RaiseTokenAdded(token);
            RaiseTokensChanged();
            return true;
        }
        finally
        {
            _isUpdatingTokens = false;
        }
    }

    private void UpdateSuggestions(string text)
    {
        FilteredSuggestions.Clear();

        if (SuggestionsSource == null || string.IsNullOrWhiteSpace(text))
        {
            HideSuggestions();
            return;
        }

        var search = text.Trim().ToLowerInvariant();

        foreach (var item in SuggestionsSource)
        {
            var suggestion = item?.ToString();
            if (string.IsNullOrEmpty(suggestion))
                continue;

            // Skip if already a token
            if (!AllowDuplicates && Tokens?.Contains(suggestion, StringComparer.OrdinalIgnoreCase) == true)
                continue;

            if (suggestion.Contains(search, StringComparison.OrdinalIgnoreCase))
            {
                FilteredSuggestions.Add(suggestion);
            }
        }

        suggestionsDropdown.IsVisible = FilteredSuggestions.Count > 0;
    }

    private void HideSuggestions()
    {
        suggestionsDropdown.IsVisible = false;
    }

    private void RaiseTokenAdded(string token)
    {
        TokenAdded?.Invoke(this, token);

        if (TokenAddedCommand?.CanExecute(token) == true)
        {
            TokenAddedCommand.Execute(token);
        }
    }

    private void RaiseTokenRemoved(string token)
    {
        TokenRemoved?.Invoke(this, token);

        if (TokenRemovedCommand?.CanExecute(token) == true)
        {
            TokenRemovedCommand.Execute(token);
        }
    }

    private void RaiseTokensChanged()
    {
        TokensChanged?.Invoke(this, Tokens?.ToList() ?? new List<string>());

        if (TokensChangedCommand?.CanExecute(Tokens) == true)
        {
            TokensChangedCommand.Execute(Tokens);
        }
    }

    private void RaiseInvalidTokenAttempted(string token)
    {
        InvalidTokenAttempted?.Invoke(this, token);

        if (InvalidTokenAttemptedCommand?.CanExecute(token) == true)
        {
            InvalidTokenAttemptedCommand.Execute(token);
        }
    }

    #endregion
}
