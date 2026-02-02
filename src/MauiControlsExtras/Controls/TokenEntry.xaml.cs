using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.ContextMenu;
using Microsoft.Maui.Controls.Shapes;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A text entry control that converts typed values into removable tokens/chips.
/// </summary>
public partial class TokenEntry : TextStyledControlBase, IValidatable, Base.IKeyboardNavigable, Base.IClipboardSupport, Base.IContextMenuSupport
{
    #region Fields

    private Entry? _inputEntry;
    private bool _isUpdatingTokens;
    private readonly List<string> _validationErrors = new();
    private string _previousText = string.Empty;
    private int _selectedTokenIndex = -1;
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();
    private readonly ContextMenuItemCollection _contextMenuItems = new();

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

    #region Clipboard Command Properties

    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(TokenEntry),
        default(ICommand));

    public static readonly BindableProperty PasteDelimitersProperty = BindableProperty.Create(
        nameof(PasteDelimiters),
        typeof(char[]),
        typeof(TokenEntry),
        new[] { ',', ';', '\n', '\t' });

    public static readonly BindableProperty ShowDefaultContextMenuProperty = BindableProperty.Create(
        nameof(ShowDefaultContextMenu),
        typeof(bool),
        typeof(TokenEntry),
        true);

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

    /// <summary>
    /// Occurs before a copy operation. Can be cancelled.
    /// </summary>
    public event EventHandler<TokenClipboardEventArgs>? Copying;

    /// <summary>
    /// Occurs before a cut operation. Can be cancelled.
    /// </summary>
    public event EventHandler<TokenClipboardEventArgs>? Cutting;

    /// <summary>
    /// Occurs before a paste operation. Can be cancelled.
    /// </summary>
    public event EventHandler<TokenClipboardEventArgs>? Pasting;

    /// <summary>
    /// Occurs after a paste operation completes, with details about skipped tokens.
    /// </summary>
    public event EventHandler<TokenClipboardEventArgs>? Pasted;

    /// <summary>
    /// Occurs before the context menu is opened. Allows customization of menu items and cancellation.
    /// </summary>
    public event EventHandler<ContextMenuOpeningEventArgs>? ContextMenuOpening;

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
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = _inputEntry?.Focus() ?? base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
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
            for (var i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];
                var isSelected = i == _selectedTokenIndex;
                var chip = CreateTokenChip(token, i, isSelected);
                tokensContainer.Children.Add(chip);
            }
        }

        // Add input entry at end
        CreateInputEntry();
        tokensContainer.Children.Add(_inputEntry!);
    }

    private View CreateTokenChip(string token, int index, bool isSelected)
    {
        var accentColor = EffectiveAccentColor ?? Colors.Gray;
        var backgroundColor = isSelected
            ? accentColor.WithAlpha(0.35f)
            : accentColor.WithAlpha(0.15f);

        var chipBorder = new Border
        {
            BackgroundColor = backgroundColor,
            StrokeThickness = isSelected ? 1.5 : 0,
            Stroke = accentColor,
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
            TextColor = accentColor,
            VerticalOptions = LayoutOptions.Center,
            FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None
        };
        Grid.SetColumn(textLabel, 0);
        chipGrid.Add(textLabel);

        var removeLabel = new Label
        {
            Text = "✕",
            FontSize = 11,
            TextColor = accentColor.WithAlpha(0.7f),
            VerticalOptions = LayoutOptions.Center
        };
        var removeTap = new TapGestureRecognizer();
        removeTap.Tapped += (s, e) => RemoveToken(token);
        removeLabel.GestureRecognizers.Add(removeTap);
        Grid.SetColumn(removeLabel, 1);
        chipGrid.Add(removeLabel);

        chipBorder.Content = chipGrid;

        // Add tap to select
        var selectTap = new TapGestureRecognizer();
        selectTap.Tapped += (s, e) =>
        {
            _selectedTokenIndex = index;
            UpdateTokenSelection();
        };
        chipBorder.GestureRecognizers.Add(selectTap);

        // Add context menu gestures
        AddContextMenuGestures(chipBorder, token, index);

        return chipBorder;
    }

    private void AddContextMenuGestures(View view, string token, int tokenIndex)
    {
#if WINDOWS
        // Windows: Use right-click via handler
        view.HandlerChanged += (s, e) =>
        {
            if (view.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
            {
                element.RightTapped += async (sender, args) =>
                {
                    args.Handled = true;
                    _selectedTokenIndex = tokenIndex;
                    UpdateTokenSelection();
                    var position = new Point(args.GetPosition(element).X, args.GetPosition(element).Y);
                    await ShowContextMenuAsync(position, token);
                };
            }
        };
#endif

        // All platforms: Long-press gesture
        var longPressRecognizer = new PointerGestureRecognizer();
        DateTime? pressStartTime = null;
        Point? pressStartPosition = null;
        CancellationTokenSource? longPressCts = null;

        longPressRecognizer.PointerPressed += (s, e) =>
        {
            pressStartTime = DateTime.UtcNow;
            var positions = e.GetPosition(view);
            pressStartPosition = positions;
            longPressCts?.Cancel();
            longPressCts = new CancellationTokenSource();

            var cts = longPressCts;
            _ = Task.Delay(500, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled && pressStartTime.HasValue)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        _selectedTokenIndex = tokenIndex;
                        UpdateTokenSelection();
                        await ShowContextMenuAsync(pressStartPosition, token);
                    });
                    pressStartTime = null;
                }
            });
        };

        longPressRecognizer.PointerMoved += (s, e) =>
        {
            if (pressStartPosition.HasValue)
            {
                var currentPos = e.GetPosition(view);
                if (currentPos.HasValue)
                {
                    var distance = Math.Sqrt(
                        Math.Pow(currentPos.Value.X - pressStartPosition.Value.X, 2) +
                        Math.Pow(currentPos.Value.Y - pressStartPosition.Value.Y, 2));

                    if (distance > 10)
                    {
                        // Movement detected, cancel long press
                        longPressCts?.Cancel();
                        pressStartTime = null;
                    }
                }
            }
        };

        longPressRecognizer.PointerReleased += (s, e) =>
        {
            longPressCts?.Cancel();
            pressStartTime = null;
            pressStartPosition = null;
        };

        view.GestureRecognizers.Add(longPressRecognizer);
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
        // Guard against app shutdown - don't try to modify UI when disposed
        if (Handler == null) return;

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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Handler != null) HideSuggestions();
            });
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

    #region IKeyboardNavigable Implementation

    /// <inheritdoc />
    public bool CanReceiveFocus => IsEnabled && IsVisible;

    /// <inheritdoc />
    public bool IsKeyboardNavigationEnabled
    {
        get => _isKeyboardNavigationEnabled;
        set
        {
            _isKeyboardNavigationEnabled = value;
            OnPropertyChanged(nameof(IsKeyboardNavigationEnabled));
        }
    }

    /// <inheritdoc />
    public bool HasKeyboardFocus => IsFocused || (_inputEntry?.IsFocused ?? false);

    /// <summary>
    /// Identifies the GotFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(TokenEntry));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(TokenEntry));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(TokenEntry));

    /// <inheritdoc />
    public ICommand? GotFocusCommand
    {
        get => (ICommand?)GetValue(GotFocusCommandProperty);
        set => SetValue(GotFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? LostFocusCommand
    {
        get => (ICommand?)GetValue(LostFocusCommandProperty);
        set => SetValue(LostFocusCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? KeyPressCommand
    {
        get => (ICommand?)GetValue(KeyPressCommandProperty);
        set => SetValue(KeyPressCommandProperty, value);
    }

    /// <inheritdoc />
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusGained;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyboardFocusEventArgs>? KeyboardFocusLost;
#pragma warning restore CS0067

    /// <inheritdoc />
    public event EventHandler<Base.KeyEventArgs>? KeyPressed;

    /// <inheritdoc />
#pragma warning disable CS0067 // Event is never used (raised by platform-specific handlers)
    public event EventHandler<Base.KeyEventArgs>? KeyReleased;
#pragma warning restore CS0067

    /// <inheritdoc />
    public bool HandleKeyPress(Base.KeyEventArgs e)
    {
        if (!IsKeyboardNavigationEnabled) return false;

        // Raise event first
        KeyPressed?.Invoke(this, e);
        if (e.Handled) return true;

        // Execute command if set
        if (KeyPressCommand?.CanExecute(e) == true)
        {
            KeyPressCommand.Execute(e);
            if (e.Handled) return true;
        }

        // Handle backspace to select/delete tokens when input is empty
        if (e.Key == "Back" && string.IsNullOrEmpty(Text))
        {
            return HandleBackspaceKey();
        }

        // Handle delete to remove selected token
        if (e.Key == "Delete" && _selectedTokenIndex >= 0)
        {
            return HandleDeleteKey();
        }

        // Handle left arrow to select tokens
        if (e.Key == "Left" && string.IsNullOrEmpty(Text))
        {
            return HandleLeftKey();
        }

        // Handle right arrow to deselect tokens
        if (e.Key == "Right" && _selectedTokenIndex >= 0)
        {
            return HandleRightKey();
        }

        // Handle A for select all tokens
        if (e.Key == "A" && e.IsPlatformCommandPressed)
        {
            return HandleSelectAllTokens();
        }

        // Handle clipboard shortcuts
        if (e.Key == "C" && e.IsPlatformCommandPressed)
        {
            return HandleCopyKey();
        }

        if (e.Key == "X" && e.IsPlatformCommandPressed)
        {
            return HandleCutKey();
        }

        if (e.Key == "V" && e.IsPlatformCommandPressed)
        {
            return HandlePasteKey();
        }

        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<Base.KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            _keyboardShortcuts.AddRange(new[]
            {
                new Base.KeyboardShortcut { Key = "Enter", Description = "Create token from current text", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Backspace", Description = "Delete last token (when input empty)", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Delete", Description = "Delete selected token", Category = "Action" },
                new Base.KeyboardShortcut { Key = "Left", Description = "Select previous token", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Right", Description = "Deselect token", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Ctrl+C", Description = "Copy selected token", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+X", Description = "Cut selected token", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+V", Description = "Paste tokens from clipboard", Category = "Clipboard" },
            });
        }
        return _keyboardShortcuts;
    }

    private bool HandleBackspaceKey()
    {
        if (Tokens != null && Tokens.Count > 0)
        {
            if (_selectedTokenIndex >= 0)
            {
                // Delete selected token
                RemoveToken(Tokens[_selectedTokenIndex]);
                _selectedTokenIndex = Math.Min(_selectedTokenIndex, Tokens.Count - 1);
            }
            else
            {
                // Select last token
                _selectedTokenIndex = Tokens.Count - 1;
            }
            return true;
        }
        return false;
    }

    private bool HandleDeleteKey()
    {
        if (Tokens != null && _selectedTokenIndex >= 0 && _selectedTokenIndex < Tokens.Count)
        {
            RemoveToken(Tokens[_selectedTokenIndex]);
            _selectedTokenIndex = Math.Min(_selectedTokenIndex, Tokens.Count - 1);
            return true;
        }
        return false;
    }

    private bool HandleLeftKey()
    {
        if (Tokens != null && Tokens.Count > 0)
        {
            _selectedTokenIndex = _selectedTokenIndex <= 0 ? Tokens.Count - 1 : _selectedTokenIndex - 1;
            return true;
        }
        return false;
    }

    private bool HandleRightKey()
    {
        if (_selectedTokenIndex >= 0 && Tokens != null)
        {
            _selectedTokenIndex = _selectedTokenIndex >= Tokens.Count - 1 ? -1 : _selectedTokenIndex + 1;
            if (_selectedTokenIndex == -1)
            {
                _inputEntry?.Focus();
            }
            return true;
        }
        return false;
    }

    private bool HandleSelectAllTokens()
    {
        // Select all is not really applicable for token entry
        // but we can return true to prevent default browser behavior
        return false;
    }

    #endregion

    #region IClipboardSupport Implementation

    /// <inheritdoc />
    public bool CanCopy => _selectedTokenIndex >= 0 && Tokens != null && Tokens.Count > _selectedTokenIndex;

    /// <inheritdoc />
    public bool CanCut => CanCopy;

    /// <inheritdoc />
    public bool CanPaste => IsEnabled && !IsMaxReached;

    /// <summary>
    /// Gets or sets the command executed when a copy operation is performed.
    /// </summary>
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a cut operation is performed.
    /// </summary>
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command executed when a paste operation is performed.
    /// </summary>
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the characters used to split clipboard text during paste operations.
    /// Default: comma, semicolon, newline, tab.
    /// </summary>
    public char[] PasteDelimiters
    {
        get => (char[])GetValue(PasteDelimitersProperty);
        set => SetValue(PasteDelimitersProperty, value);
    }

    /// <inheritdoc />
    public void Copy()
    {
        if (!CanCopy) return;

        var token = Tokens![_selectedTokenIndex];
        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Copy, token);

        Copying?.Invoke(this, args);
        if (args.Cancel) return;

        Clipboard.Default.SetTextAsync(token).ConfigureAwait(false);

        if (CopyCommand?.CanExecute(token) == true)
        {
            CopyCommand.Execute(token);
        }
    }

    /// <inheritdoc />
    public void Cut()
    {
        if (!CanCut) return;

        var token = Tokens![_selectedTokenIndex];
        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Cut, token);

        Cutting?.Invoke(this, args);
        if (args.Cancel) return;

        Clipboard.Default.SetTextAsync(token).ConfigureAwait(false);

        var removedIndex = _selectedTokenIndex;
        RemoveToken(token);

        // Adjust selection after removal
        if (Tokens != null && Tokens.Count > 0)
        {
            _selectedTokenIndex = Math.Min(removedIndex, Tokens.Count - 1);
        }
        else
        {
            _selectedTokenIndex = -1;
        }

        UpdateTokenSelection();

        if (CutCommand?.CanExecute(token) == true)
        {
            CutCommand.Execute(token);
        }
    }

    /// <inheritdoc />
    public void Paste()
    {
        _ = PasteAsync();
    }

    /// <summary>
    /// Asynchronously pastes tokens from the clipboard.
    /// </summary>
    public async Task PasteAsync()
    {
        if (!CanPaste) return;

        var clipboardText = await Clipboard.Default.GetTextAsync();
        if (string.IsNullOrWhiteSpace(clipboardText)) return;

        var tokenCandidates = clipboardText
            .Split(PasteDelimiters, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        if (tokenCandidates.Count == 0) return;

        var args = new TokenClipboardEventArgs(TokenClipboardOperation.Paste, tokenCandidates, clipboardText);

        Pasting?.Invoke(this, args);
        if (args.Cancel) return;

        var addedTokens = new List<string>();
        var skippedTokens = new List<string>();
        var skipReasons = new Dictionary<string, string>();

        foreach (var token in tokenCandidates)
        {
            var processedToken = token;

            // Check max tokens limit
            if (IsMaxReached)
            {
                skippedTokens.Add(token);
                skipReasons[token] = "Maximum token count reached";
                continue;
            }

            // Truncate if exceeds max length
            if (MaxTokenLength.HasValue && processedToken.Length > MaxTokenLength.Value)
            {
                processedToken = processedToken[..MaxTokenLength.Value];
            }

            // Check for duplicates
            if (!AllowDuplicates && Tokens?.Contains(processedToken, StringComparer.OrdinalIgnoreCase) == true)
            {
                skippedTokens.Add(token);
                skipReasons[token] = "Duplicate token not allowed";
                continue;
            }

            // Custom validation
            if (ValidationFunc != null && !ValidationFunc(processedToken))
            {
                skippedTokens.Add(token);
                skipReasons[token] = "Failed custom validation";
                continue;
            }

            if (AddToken(processedToken))
            {
                addedTokens.Add(processedToken);
            }
            else
            {
                skippedTokens.Add(token);
                skipReasons[token] = "Failed to add token";
            }
        }

        // Raise Pasted event with results
        var pastedArgs = new TokenClipboardEventArgs(TokenClipboardOperation.Paste, addedTokens, clipboardText)
        {
            SkippedTokens = skippedTokens,
            SkipReasons = skipReasons,
            SuccessCount = addedTokens.Count
        };

        Pasted?.Invoke(this, pastedArgs);

        if (PasteCommand?.CanExecute(pastedArgs) == true)
        {
            PasteCommand.Execute(pastedArgs);
        }
    }

    /// <inheritdoc />
    public object? GetClipboardContent()
    {
        if (!CanCopy) return null;
        return Tokens![_selectedTokenIndex];
    }

    private void UpdateTokenSelection()
    {
        RebuildTokenChips();
        OnPropertyChanged(nameof(CanCopy));
        OnPropertyChanged(nameof(CanCut));
    }

    private bool HandleCopyKey()
    {
        if (CanCopy)
        {
            Copy();
            return true;
        }
        return false;
    }

    private bool HandleCutKey()
    {
        if (CanCut)
        {
            Cut();
            return true;
        }
        return false;
    }

    private bool HandlePasteKey()
    {
        if (CanPaste)
        {
            Paste();
            return true;
        }
        return false;
    }

    #endregion

    #region IContextMenuSupport Implementation

    /// <inheritdoc />
    public ContextMenuItemCollection ContextMenuItems => _contextMenuItems;

    /// <summary>
    /// Gets or sets whether to show default context menu items (Copy, Cut, Paste).
    /// </summary>
    public bool ShowDefaultContextMenu
    {
        get => (bool)GetValue(ShowDefaultContextMenuProperty);
        set => SetValue(ShowDefaultContextMenuProperty, value);
    }

    /// <inheritdoc />
    public void ShowContextMenu(Point? position = null)
    {
        _ = ShowContextMenuAsync(position);
    }

    /// <summary>
    /// Asynchronously shows the context menu.
    /// </summary>
    /// <param name="position">The position to show the menu at.</param>
    /// <param name="targetToken">The token that triggered the context menu, if any.</param>
    public async Task ShowContextMenuAsync(Point? position = null, string? targetToken = null)
    {
        var menuItems = new ContextMenuItemCollection();

        // Add custom items first
        foreach (var item in _contextMenuItems)
        {
            menuItems.Add(item);
        }

        // Add separator if we have custom items and will add default items
        if (_contextMenuItems.Count > 0 && ShowDefaultContextMenu)
        {
            menuItems.AddSeparator();
        }

        // Add default clipboard items
        if (ShowDefaultContextMenu)
        {
            var copyItem = ContextMenuItem.Create(
                "Copy",
                Copy,
                "\uE8C8", // Copy icon
                GetPlatformShortcutText("C"));
            copyItem.IsEnabled = CanCopy;
            menuItems.Add(copyItem);

            var cutItem = ContextMenuItem.Create(
                "Cut",
                Cut,
                "\uE8C6", // Cut icon
                GetPlatformShortcutText("X"));
            cutItem.IsEnabled = CanCut;
            menuItems.Add(cutItem);

            var pasteItem = ContextMenuItem.Create(
                "Paste",
                Paste,
                "\uE77F", // Paste icon
                GetPlatformShortcutText("V"));
            pasteItem.IsEnabled = CanPaste;
            menuItems.Add(pasteItem);
        }

        if (menuItems.Count == 0) return;

        // Raise opening event
        var eventArgs = new ContextMenuOpeningEventArgs(
            menuItems,
            position ?? Point.Zero,
            this,
            targetToken);

        ContextMenuOpening?.Invoke(this, eventArgs);

        if (eventArgs.Cancel) return;
        if (eventArgs.Handled) return;

        await ContextMenuService.Current.ShowAsync(this, menuItems.ToList(), position);
    }

    private static string GetPlatformShortcutText(string key)
    {
#if MACCATALYST || IOS
        return $"⌘{key}";
#else
        return $"Ctrl+{key}";
#endif
    }

    #endregion
}
