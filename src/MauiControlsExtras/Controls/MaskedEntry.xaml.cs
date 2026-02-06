using System.Text;
using System.Windows.Input;
using MauiControlsExtras.Base;
using MauiControlsExtras.Base.Validation;
using MauiControlsExtras.Helpers;

namespace MauiControlsExtras.Controls;

/// <summary>
/// A text entry control with input masking for formatted data.
/// </summary>
public partial class MaskedEntry : TextStyledControlBase, IValidatable, Base.IKeyboardNavigable, Base.IClipboardSupport
{
    #region Predefined Masks

    /// <summary>US phone number: (000) 000-0000</summary>
    public const string PhoneUS = "(000) 000-0000";

    /// <summary>International phone: +00 000 000 0000</summary>
    public const string PhoneIntl = "+00 000 000 0000";

    /// <summary>Credit card: 0000 0000 0000 0000</summary>
    public const string CreditCard = "0000 0000 0000 0000";

    /// <summary>US date: 00/00/0000</summary>
    public const string DateUS = "00/00/0000";

    /// <summary>ISO date: 0000-00-00</summary>
    public const string DateISO = "0000-00-00";

    /// <summary>Time: 00:00</summary>
    public const string TimeHHMM = "00:00";

    /// <summary>Time with seconds: 00:00:00</summary>
    public const string TimeHHMMSS = "00:00:00";

    /// <summary>Social Security Number: 000-00-0000</summary>
    public const string SSN = "000-00-0000";

    /// <summary>US ZIP code: 00000-9999</summary>
    public const string ZipUS = "00000-9999";

    /// <summary>Canadian postal code: A0A 0A0</summary>
    public const string ZipCA = "A0A 0A0";

    #endregion

    #region Fields

    private bool _isUpdatingText;
    private readonly List<string> _validationErrors = new();
    private List<MaskToken> _maskTokens = new();
    private bool _isKeyboardNavigationEnabled = true;
    private static readonly List<Base.KeyboardShortcut> _keyboardShortcuts = new();

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MaskedEntry),
        default(string),
        BindingMode.TwoWay,
        propertyChanged: OnTextChanged);

    public static readonly BindableProperty MaskProperty = BindableProperty.Create(
        nameof(Mask),
        typeof(string),
        typeof(MaskedEntry),
        default(string),
        propertyChanged: OnMaskChanged);

    public static readonly BindableProperty PromptCharProperty = BindableProperty.Create(
        nameof(PromptChar),
        typeof(char),
        typeof(MaskedEntry),
        '_');

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(MaskedEntry),
        default(string));

    public static readonly BindableProperty IncludeLiteralsProperty = BindableProperty.Create(
        nameof(IncludeLiterals),
        typeof(bool),
        typeof(MaskedEntry),
        false,
        propertyChanged: OnIncludeLiteralsChanged);

    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
        nameof(IsPassword),
        typeof(bool),
        typeof(MaskedEntry),
        false);

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(MaskedEntry),
        false);

    public static readonly BindableProperty RequiredErrorMessageProperty = BindableProperty.Create(
        nameof(RequiredErrorMessage),
        typeof(string),
        typeof(MaskedEntry),
        "This field is required.");

    public static readonly BindableProperty ShowValidationIconProperty = BindableProperty.Create(
        nameof(ShowValidationIcon),
        typeof(bool),
        typeof(MaskedEntry),
        true);

    #endregion

    #region Command Properties

    public static readonly BindableProperty TextChangedCommandProperty = BindableProperty.Create(
        nameof(TextChangedCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    public static readonly BindableProperty CompletedCommandProperty = BindableProperty.Create(
        nameof(CompletedCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    public static readonly BindableProperty ValidateCommandProperty = BindableProperty.Create(
        nameof(ValidateCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    public static readonly BindableProperty CutCommandProperty = BindableProperty.Create(
        nameof(CutCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    public static readonly BindableProperty PasteCommandProperty = BindableProperty.Create(
        nameof(PasteCommand),
        typeof(ICommand),
        typeof(MaskedEntry),
        default(ICommand));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the raw unmasked text value.
    /// </summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets the formatted masked text.
    /// </summary>
    public string MaskedText => GetMaskedText();

    /// <summary>
    /// Gets or sets the mask pattern.
    /// </summary>
    public string? Mask
    {
        get => (string?)GetValue(MaskProperty);
        set => SetValue(MaskProperty, value);
    }

    /// <summary>
    /// Gets or sets the prompt character for unfilled positions.
    /// </summary>
    public char PromptChar
    {
        get => (char)GetValue(PromptCharProperty);
        set => SetValue(PromptCharProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to include literal characters in the Text value.
    /// </summary>
    public bool IncludeLiterals
    {
        get => (bool)GetValue(IncludeLiteralsProperty);
        set => SetValue(IncludeLiteralsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to obscure the input.
    /// </summary>
    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    /// <summary>
    /// Gets or sets whether this field is required.
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
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
    /// Gets or sets whether to show the validation icon.
    /// </summary>
    public bool ShowValidationIcon
    {
        get => (bool)GetValue(ShowValidationIconProperty);
        set => SetValue(ShowValidationIconProperty, value);
    }

    /// <summary>
    /// Gets whether the mask is completely filled.
    /// </summary>
    public bool IsMaskComplete => CheckMaskComplete();

    /// <summary>
    /// Gets the display text for the entry.
    /// </summary>
    public string DisplayText => GetDisplayText();

    /// <summary>
    /// Gets the appropriate keyboard type based on the mask.
    /// </summary>
    public Keyboard EntryKeyboard => GetKeyboardType();

    /// <summary>
    /// Gets the current border color based on state.
    /// </summary>
    public Color CurrentBorderColor
    {
        get
        {
            if (!IsValid)
                return EffectiveErrorBorderColor;
            if (entry?.IsFocused == true)
                return EffectiveFocusBorderColor;
            return EffectiveBorderColor;
        }
    }

    /// <summary>
    /// Gets the validation icon text.
    /// </summary>
    public string ValidationIconText
    {
        get
        {
            if (!ShowValidationIcon) return string.Empty;
            if (string.IsNullOrEmpty(Text)) return string.Empty;
            return IsMaskComplete ? "✓" : "⚠";
        }
    }

    /// <summary>
    /// Gets the validation icon color.
    /// </summary>
    public Color ValidationIconColor =>
        IsMaskComplete ? EffectiveSuccessColor : EffectiveWarningColor;

    #endregion

    #region Command Properties (CLR)

    public ICommand? TextChangedCommand
    {
        get => (ICommand?)GetValue(TextChangedCommandProperty);
        set => SetValue(TextChangedCommandProperty, value);
    }

    public ICommand? CompletedCommand
    {
        get => (ICommand?)GetValue(CompletedCommandProperty);
        set => SetValue(CompletedCommandProperty, value);
    }

    public ICommand? ValidateCommand
    {
        get => (ICommand?)GetValue(ValidateCommandProperty);
        set => SetValue(ValidateCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? CutCommand
    {
        get => (ICommand?)GetValue(CutCommandProperty);
        set => SetValue(CutCommandProperty, value);
    }

    /// <inheritdoc />
    public ICommand? PasteCommand
    {
        get => (ICommand?)GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    #endregion

    #region IClipboardSupport

    /// <inheritdoc />
    public bool CanCopy => IsEnabled && entry?.Text?.Length > 0;

    /// <inheritdoc />
    public bool CanCut => CanCopy;

    /// <inheritdoc />
    public bool CanPaste => IsEnabled;

    /// <inheritdoc />
    public void Copy()
    {
        if (!CanCopy) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        CopyCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Cut()
    {
        if (!CanCut) return;
        var content = GetClipboardContent();
        if (content is string text)
            Clipboard.Default.SetTextAsync(text).ConfigureAwait(false);
        entry.Text = string.Empty;
        CutCommand?.Execute(content);
    }

    /// <inheritdoc />
    public void Paste()
    {
        if (!CanPaste) return;
        var task = Clipboard.Default.GetTextAsync();
        task.ContinueWith(t =>
        {
            if (t.Result is string text)
                MainThread.BeginInvokeOnMainThread(() => entry.Text = text);
        }, TaskScheduler.Default);
        PasteCommand?.Execute(null);
    }

    /// <inheritdoc />
    public object? GetClipboardContent() => entry?.Text;

    #endregion

    #region IValidatable

    public bool IsValid => _validationErrors.Count == 0;

    public IReadOnlyList<string> ValidationErrors => _validationErrors.AsReadOnly();

    public ValidationResult Validate()
    {
        var wasValid = IsValid;

        _validationErrors.Clear();

        if (IsRequired && string.IsNullOrEmpty(Text))
        {
            _validationErrors.Add(RequiredErrorMessage);
        }
        else if (!string.IsNullOrEmpty(Text) && !IsMaskComplete)
        {
            _validationErrors.Add("Please complete the required format.");
        }

        var isNowValid = IsValid;
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(ValidationErrors));
        OnPropertyChanged(nameof(CurrentBorderColor));
        OnPropertyChanged(nameof(ValidationIconText));
        OnPropertyChanged(nameof(ValidationIconColor));

        // Raise ValidationChanged if state changed
        if (wasValid != isNowValid)
        {
            ValidationChanged?.Invoke(this, isNowValid);
        }

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
    /// Occurs when the text changes.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>
    /// Occurs when the mask is completely filled.
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Occurs when IsValid changes.
    /// </summary>
    public event EventHandler<bool>? ValidationChanged;

    #endregion

    #region Constructor

    public MaskedEntry()
    {
        InitializeComponent();
        ParseMask();
        entry.HandlerChanged += OnEntryHandlerChanged;
    }

    #endregion

    #region Handler Changed

    private void OnEntryHandlerChanged(object? sender, EventArgs e)
    {
        if (entry.Handler?.PlatformView == null) return;
        MobileClipboardBridge.Setup(entry, this);
    }

    #endregion

    #region Property Changed Handlers

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MaskedEntry control && !control._isUpdatingText)
        {
            control.OnPropertyChanged(nameof(MaskedText));
            control.OnPropertyChanged(nameof(DisplayText));
            control.OnPropertyChanged(nameof(IsMaskComplete));
            control.OnPropertyChanged(nameof(ValidationIconText));
            control.OnPropertyChanged(nameof(ValidationIconColor));
            control.UpdateEntryText();
            control.RaiseTextChanged((string?)oldValue, (string?)newValue);
        }
    }

    private static void OnMaskChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MaskedEntry control)
        {
            control.ParseMask();
            control.OnPropertyChanged(nameof(MaskedText));
            control.OnPropertyChanged(nameof(DisplayText));
            control.OnPropertyChanged(nameof(IsMaskComplete));
            control.OnPropertyChanged(nameof(EntryKeyboard));
            control.UpdateEntryText();
        }
    }

    private static void OnIncludeLiteralsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MaskedEntry control)
        {
            control.OnPropertyChanged(nameof(Text));
        }
    }

    #endregion

    #region Event Handlers

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingText) return;

        _isUpdatingText = true;
        try
        {
            var previousRawText = Text ?? string.Empty;
            var newRawText = ExtractRawText(e.NewTextValue);

            // Some mobile keyboards emit replacement-style updates that only contain
            // the latest typed character instead of the full masked display text.
            if ((e.NewTextValue?.Length ?? 0) == 1 &&
                (e.OldTextValue?.Length ?? 0) > 1 &&
                newRawText.Length <= 1 &&
                previousRawText.Length < GetMaxRawInputLength() &&
                TryNormalizeInputForRawIndex(e.NewTextValue![0], previousRawText.Length, out var normalizedDeltaChar))
            {
                newRawText = previousRawText + normalizedDeltaChar;
            }

            Text = TrimToMaskCapacity(newRawText);

            // Immediately update entry to show masked text (filtered)
            // This must be done here because OnTextChanged won't run while _isUpdatingText is true
            if (entry != null)
            {
                var maskedText = DisplayText;
                var textWasRewritten = !string.Equals(entry.Text, maskedText, StringComparison.Ordinal);
                if (textWasRewritten)
                {
                    entry.Text = maskedText;
                    if (IsMobilePlatform())
                    {
                        var desiredCursor = GetDisplayCursorPositionForRawLength((Text ?? string.Empty).Length, maskedText);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (entry == null) return;
                            entry.CursorPosition = Math.Clamp(desiredCursor, 0, entry.Text?.Length ?? 0);
                            entry.SelectionLength = 0;
                        });
                    }
                }
            }
        }
        finally
        {
            _isUpdatingText = false;
        }
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentBorderColor));
        UpdateEntryText();
    }

    private void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentBorderColor));
        Validate();

        // Clear display if empty
        if (string.IsNullOrEmpty(Text))
        {
            _isUpdatingText = true;
            entry.Text = string.Empty;
            _isUpdatingText = false;
            return;
        }

        UpdateEntryText();
    }

    private void OnEntryCompleted(object? sender, EventArgs e)
    {
        if (IsMaskComplete)
        {
            Completed?.Invoke(this, EventArgs.Empty);

            if (CompletedCommand?.CanExecute(Text) == true)
            {
                CompletedCommand.Execute(Text);
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Clears the text.
    /// </summary>
    public void Clear()
    {
        Text = null;
    }

    /// <summary>
    /// Focuses the entry.
    /// </summary>
    public new bool Focus()
    {
        if (!CanReceiveFocus) return false;

        var result = entry?.Focus() ?? base.Focus();
        if (result)
        {
            KeyboardFocusGained?.Invoke(this, new Base.KeyboardFocusEventArgs(true));
            GotFocusCommand?.Execute(this);
        }
        return result;
    }

    /// <summary>
    /// Unfocuses the entry.
    /// </summary>
    public new void Unfocus()
    {
        entry?.Unfocus();
    }

    #endregion

    #region Private Methods - Mask Parsing

    private void ParseMask()
    {
        _maskTokens.Clear();

        if (string.IsNullOrEmpty(Mask))
            return;

        var i = 0;
        while (i < Mask.Length)
        {
            var c = Mask[i];

            // Escape character
            if (c == '\\' && i + 1 < Mask.Length)
            {
                _maskTokens.Add(new MaskToken(MaskTokenType.Literal, Mask[i + 1]));
                i += 2;
                continue;
            }

            switch (c)
            {
                case '0':
                    _maskTokens.Add(new MaskToken(MaskTokenType.RequiredDigit, c));
                    break;
                case '9':
                    _maskTokens.Add(new MaskToken(MaskTokenType.OptionalDigit, c));
                    break;
                case 'A':
                    _maskTokens.Add(new MaskToken(MaskTokenType.RequiredLetter, c));
                    break;
                case 'a':
                    _maskTokens.Add(new MaskToken(MaskTokenType.OptionalLetter, c));
                    break;
                case 'L':
                    _maskTokens.Add(new MaskToken(MaskTokenType.RequiredLetterUpper, c));
                    break;
                case '?':
                    _maskTokens.Add(new MaskToken(MaskTokenType.OptionalLetterUpper, c));
                    break;
                case '&':
                    _maskTokens.Add(new MaskToken(MaskTokenType.RequiredAny, c));
                    break;
                case 'C':
                    _maskTokens.Add(new MaskToken(MaskTokenType.OptionalAny, c));
                    break;
                default:
                    _maskTokens.Add(new MaskToken(MaskTokenType.Literal, c));
                    break;
            }

            i++;
        }
    }

    private string GetMaskedText()
    {
        if (_maskTokens.Count == 0 || string.IsNullOrEmpty(Text))
            return Text ?? string.Empty;

        var showOptionalPrompts = entry?.IsFocused == true;
        var result = new StringBuilder();
        var textIndex = 0;
        var text = Text ?? string.Empty;

        foreach (var token in _maskTokens)
        {
            if (token.Type == MaskTokenType.Literal)
            {
                result.Append(token.Character);
            }
            else if (textIndex < text.Length)
            {
                var inputChar = text[textIndex];
                if (ValidateChar(inputChar, token.Type, out var outputChar))
                {
                    result.Append(outputChar);
                    textIndex++;
                }
                else if (token.IsOptional)
                {
                    result.Append(PromptChar);
                }
                else
                {
                    // Skip invalid character, try next
                    textIndex++;
                }
            }
            else if (!token.IsOptional || showOptionalPrompts)
            {
                result.Append(PromptChar);
            }
        }

        return result.ToString();
    }

    private string GetDisplayText()
    {
        if (_maskTokens.Count == 0)
            return Text ?? string.Empty;

        if (string.IsNullOrEmpty(Text) && entry?.IsFocused != true)
            return string.Empty;

        return GetMaskedText();
    }

    private string ExtractRawText(string? maskedInput)
    {
        if (string.IsNullOrEmpty(maskedInput) || _maskTokens.Count == 0)
            return maskedInput ?? string.Empty;

        var result = new StringBuilder();
        var tokenIndex = 0;

        foreach (var c in maskedInput)
        {
            if (c == PromptChar)
                continue;

            while (tokenIndex < _maskTokens.Count && _maskTokens[tokenIndex].Type == MaskTokenType.Literal)
            {
                var literal = _maskTokens[tokenIndex].Character;
                if (c == literal)
                {
                    if (IncludeLiterals)
                        result.Append(c);
                    tokenIndex++;
                    goto NextInputCharacter;
                }

                tokenIndex++;
            }

            if (tokenIndex >= _maskTokens.Count)
                break;

            var token = _maskTokens[tokenIndex];
            if (ValidateChar(c, token.Type, out var outputChar))
            {
                result.Append(outputChar);
                tokenIndex++;
            }

        NextInputCharacter:
            if (result.Length >= GetMaxRawInputLength())
                break;
        }

        return TrimToMaskCapacity(result.ToString());
    }

    private bool IsLiteralAtPosition(int position)
    {
        if (position < 0 || position >= _maskTokens.Count)
            return false;
        return _maskTokens[position].Type == MaskTokenType.Literal;
    }

    private bool ValidateChar(char c, MaskTokenType tokenType, out char output)
    {
        output = c;

        switch (tokenType)
        {
            case MaskTokenType.RequiredDigit:
            case MaskTokenType.OptionalDigit:
                return char.IsDigit(c);

            case MaskTokenType.RequiredLetter:
            case MaskTokenType.OptionalLetter:
                return char.IsLetter(c);

            case MaskTokenType.RequiredLetterUpper:
            case MaskTokenType.OptionalLetterUpper:
                if (char.IsLetter(c))
                {
                    output = char.ToUpper(c);
                    return true;
                }
                return false;

            case MaskTokenType.RequiredAny:
            case MaskTokenType.OptionalAny:
                return true;

            default:
                return false;
        }
    }

    private bool CheckMaskComplete()
    {
        if (_maskTokens.Count == 0)
            return !string.IsNullOrEmpty(Text);

        var text = Text ?? string.Empty;
        var textIndex = 0;

        foreach (var token in _maskTokens)
        {
            if (token.Type == MaskTokenType.Literal)
                continue;

            if (token.IsOptional)
                continue;

            if (textIndex >= text.Length)
                return false;

            if (!ValidateChar(text[textIndex], token.Type, out _))
                return false;

            textIndex++;
        }

        return true;
    }

    private void UpdateEntryText()
    {
        if (entry == null) return;

        _isUpdatingText = true;
        entry.Text = DisplayText;
        _isUpdatingText = false;
    }

    private bool TryNormalizeInputForRawIndex(char value, int rawIndex, out char normalized)
    {
        normalized = value;
        if (_maskTokens.Count == 0)
            return true;

        var token = GetInputMaskTokenAtRawIndex(rawIndex);
        if (!token.HasValue)
            return false;

        return ValidateChar(value, token.Value.Type, out normalized);
    }

    private MaskToken? GetInputMaskTokenAtRawIndex(int rawIndex)
    {
        if (rawIndex < 0)
            return null;

        var inputIndex = 0;
        foreach (var token in _maskTokens)
        {
            if (token.Type == MaskTokenType.Literal)
                continue;

            if (inputIndex == rawIndex)
                return token;

            inputIndex++;
        }

        return null;
    }

    private int GetMaxRawInputLength()
    {
        if (_maskTokens.Count == 0)
            return int.MaxValue;

        return _maskTokens.Count(t => t.Type != MaskTokenType.Literal);
    }

    private string TrimToMaskCapacity(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;

        var max = GetMaxRawInputLength();
        return rawText.Length <= max ? rawText : rawText[..max];
    }

    private int GetDisplayCursorPositionForRawLength(int rawLength, string maskedText)
    {
        if (_maskTokens.Count == 0)
            return Math.Clamp(rawLength, 0, maskedText.Length);

        var targetRawLength = Math.Max(0, rawLength);
        var rawIndex = 0;

        for (var displayIndex = 0; displayIndex < _maskTokens.Count && displayIndex < maskedText.Length; displayIndex++)
        {
            if (_maskTokens[displayIndex].Type == MaskTokenType.Literal)
                continue;

            if (rawIndex == targetRawLength)
                return displayIndex;

            rawIndex++;
        }

        return maskedText.Length;
    }

    private static bool IsMobilePlatform()
    {
        var platform = DeviceInfo.Current.Platform;
        return platform == DevicePlatform.Android || platform == DevicePlatform.iOS;
    }

    private Keyboard GetKeyboardType()
    {
        if (_maskTokens.Count == 0)
            return Keyboard.Default;

        // Check if mask is digits-only
        var hasNonDigit = _maskTokens.Any(t =>
            t.Type != MaskTokenType.Literal &&
            t.Type != MaskTokenType.RequiredDigit &&
            t.Type != MaskTokenType.OptionalDigit);

        return hasNonDigit ? Keyboard.Default : Keyboard.Numeric;
    }

    private void RaiseTextChanged(string? oldValue, string? newValue)
    {
        var args = new TextChangedEventArgs(oldValue, newValue);
        TextChanged?.Invoke(this, args);

        if (TextChangedCommand?.CanExecute(newValue) == true)
        {
            TextChangedCommand.Execute(newValue);
        }

        // Check if now complete
        if (IsMaskComplete && !string.IsNullOrEmpty(newValue))
        {
            Completed?.Invoke(this, EventArgs.Empty);

            if (CompletedCommand?.CanExecute(newValue) == true)
            {
                CompletedCommand.Execute(newValue);
            }
        }
    }

    #endregion

    #region Inner Types

    private enum MaskTokenType
    {
        Literal,
        RequiredDigit,      // 0
        OptionalDigit,      // 9
        RequiredLetter,     // A
        OptionalLetter,     // a
        RequiredLetterUpper, // L
        OptionalLetterUpper, // ?
        RequiredAny,        // &
        OptionalAny         // C
    }

    private readonly struct MaskToken
    {
        public MaskTokenType Type { get; }
        public char Character { get; }

        public bool IsOptional => Type is
            MaskTokenType.OptionalDigit or
            MaskTokenType.OptionalLetter or
            MaskTokenType.OptionalLetterUpper or
            MaskTokenType.OptionalAny;

        public MaskToken(MaskTokenType type, char character)
        {
            Type = type;
            Character = character;
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
    public bool HasKeyboardFocus => IsFocused;

    /// <summary>
    /// Identifies the GotFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty GotFocusCommandProperty = BindableProperty.Create(
        nameof(GotFocusCommand),
        typeof(ICommand),
        typeof(MaskedEntry));

    /// <summary>
    /// Identifies the LostFocusCommand bindable property.
    /// </summary>
    public static readonly BindableProperty LostFocusCommandProperty = BindableProperty.Create(
        nameof(LostFocusCommand),
        typeof(ICommand),
        typeof(MaskedEntry));

    /// <summary>
    /// Identifies the KeyPressCommand bindable property.
    /// </summary>
    public static readonly BindableProperty KeyPressCommandProperty = BindableProperty.Create(
        nameof(KeyPressCommand),
        typeof(ICommand),
        typeof(MaskedEntry));

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

        // MaskedEntry primarily delegates to the underlying Entry
        // but we can handle clipboard shortcuts here
        return e.Key switch
        {
            "C" when e.IsPlatformCommandPressed => HandleCopyKey(),
            "X" when e.IsPlatformCommandPressed => HandleCutKey(),
            "V" when e.IsPlatformCommandPressed => HandlePasteKey(),
            _ => false
        };
    }

    private bool HandleCopyKey() { Copy(); return CanCopy; }
    private bool HandleCutKey() { Cut(); return CanCut; }
    private bool HandlePasteKey() { Paste(); return CanPaste; }

    /// <inheritdoc />
    public IReadOnlyList<Base.KeyboardShortcut> GetKeyboardShortcuts()
    {
        if (_keyboardShortcuts.Count == 0)
        {
            _keyboardShortcuts.AddRange(new[]
            {
                new Base.KeyboardShortcut { Key = "Tab", Description = "Move to next field", Category = "Navigation" },
                new Base.KeyboardShortcut { Key = "Backspace", Description = "Delete previous character", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Delete", Description = "Delete next character", Category = "Editing" },
                new Base.KeyboardShortcut { Key = "Ctrl+C", Description = "Copy", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+X", Description = "Cut", Category = "Clipboard" },
                new Base.KeyboardShortcut { Key = "Ctrl+V", Description = "Paste", Category = "Clipboard" },
            });
        }
        return _keyboardShortcuts;
    }

    #endregion
}
