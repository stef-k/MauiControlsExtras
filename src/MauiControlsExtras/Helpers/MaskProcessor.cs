using System.Text;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// Pure mask-processing logic extracted from MaskedEntry for testability.
/// No MAUI dependencies — fully unit-testable.
/// </summary>
public sealed class MaskProcessor
{
    private readonly List<MaskToken> _tokens = new();
    private char _promptChar;
    private bool _includeLiterals;

    public MaskProcessor(char promptChar = '_', bool includeLiterals = false)
    {
        _promptChar = promptChar;
        _includeLiterals = includeLiterals;
    }

    public char PromptChar
    {
        get => _promptChar;
        set => _promptChar = value;
    }

    public bool IncludeLiterals
    {
        get => _includeLiterals;
        set => _includeLiterals = value;
    }

    public IReadOnlyList<MaskToken> Tokens => _tokens;

    #region Mask Parsing

    /// <summary>
    /// Parses a mask string into tokens.
    /// </summary>
    public void ParseMask(string? mask)
    {
        _tokens.Clear();

        if (string.IsNullOrEmpty(mask))
            return;

        var i = 0;
        while (i < mask.Length)
        {
            var c = mask[i];

            // Escape character
            if (c == '\\' && i + 1 < mask.Length)
            {
                _tokens.Add(new MaskToken(MaskTokenType.Literal, mask[i + 1]));
                i += 2;
                continue;
            }

            switch (c)
            {
                case '0':
                    _tokens.Add(new MaskToken(MaskTokenType.RequiredDigit, c));
                    break;
                case '9':
                    _tokens.Add(new MaskToken(MaskTokenType.OptionalDigit, c));
                    break;
                case 'A':
                    _tokens.Add(new MaskToken(MaskTokenType.RequiredLetter, c));
                    break;
                case 'a':
                    _tokens.Add(new MaskToken(MaskTokenType.OptionalLetter, c));
                    break;
                case 'L':
                    _tokens.Add(new MaskToken(MaskTokenType.RequiredLetterUpper, c));
                    break;
                case '?':
                    _tokens.Add(new MaskToken(MaskTokenType.OptionalLetterUpper, c));
                    break;
                case '&':
                    _tokens.Add(new MaskToken(MaskTokenType.RequiredAny, c));
                    break;
                case 'C':
                    _tokens.Add(new MaskToken(MaskTokenType.OptionalAny, c));
                    break;
                default:
                    _tokens.Add(new MaskToken(MaskTokenType.Literal, c));
                    break;
            }

            i++;
        }
    }

    #endregion

    #region Core Operations

    /// <summary>
    /// Extracts raw (user-input-only) text from a masked display string.
    /// Used for desktop input and as a fallback for mobile.
    /// </summary>
    public string ExtractRawText(string? maskedInput)
    {
        if (string.IsNullOrEmpty(maskedInput) || _tokens.Count == 0)
            return maskedInput ?? string.Empty;

        var result = new StringBuilder();
        var tokenIndex = 0;

        foreach (var c in maskedInput)
        {
            if (c == _promptChar)
                continue;

            while (tokenIndex < _tokens.Count && _tokens[tokenIndex].Type == MaskTokenType.Literal)
            {
                var literal = _tokens[tokenIndex].Character;
                if (c == literal)
                {
                    if (_includeLiterals)
                        result.Append(c);
                    tokenIndex++;
                    goto NextInputCharacter;
                }

                tokenIndex++;
            }

            if (tokenIndex >= _tokens.Count)
                break;

            var token = _tokens[tokenIndex];
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

    /// <summary>
    /// Formats raw text through the mask to produce display text.
    /// </summary>
    public string GetMaskedText(string? rawText, bool showOptionalPrompts)
    {
        if (_tokens.Count == 0 || string.IsNullOrEmpty(rawText))
            return rawText ?? string.Empty;

        var result = new StringBuilder();
        var textIndex = 0;
        var text = rawText ?? string.Empty;

        foreach (var token in _tokens)
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
                    result.Append(_promptChar);
                }
                else
                {
                    // Skip invalid character, try next
                    textIndex++;
                }
            }
            else if (!token.IsOptional || showOptionalPrompts)
            {
                result.Append(_promptChar);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Validates a character against a mask token type, optionally normalizing (e.g. uppercase).
    /// </summary>
    public bool ValidateChar(char c, MaskTokenType tokenType, out char output)
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

    /// <summary>
    /// Checks whether all required mask positions are filled.
    /// </summary>
    public bool CheckMaskComplete(string? rawText)
    {
        if (_tokens.Count == 0)
            return !string.IsNullOrEmpty(rawText);

        var text = rawText ?? string.Empty;
        var textIndex = 0;

        foreach (var token in _tokens)
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

    /// <summary>
    /// Returns the maximum number of raw input characters the mask can accept.
    /// </summary>
    public int GetMaxRawInputLength()
    {
        if (_tokens.Count == 0)
            return int.MaxValue;

        return _tokens.Count(t => t.Type != MaskTokenType.Literal);
    }

    /// <summary>
    /// Trims raw text to mask capacity.
    /// </summary>
    public string TrimToMaskCapacity(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;

        var max = GetMaxRawInputLength();
        return rawText.Length <= max ? rawText : rawText[..max];
    }

    /// <summary>
    /// Maps a raw text length to the corresponding cursor position in the display string.
    /// </summary>
    public int GetDisplayCursorPositionForRawLength(int rawLength, string maskedText)
    {
        if (_tokens.Count == 0)
            return Math.Clamp(rawLength, 0, maskedText.Length);

        var targetRawLength = Math.Max(0, rawLength);
        var rawIndex = 0;

        for (var displayIndex = 0; displayIndex < _tokens.Count && displayIndex < maskedText.Length; displayIndex++)
        {
            if (_tokens[displayIndex].Type == MaskTokenType.Literal)
                continue;

            if (rawIndex == targetRawLength)
                return displayIndex;

            rawIndex++;
        }

        return maskedText.Length;
    }

    /// <summary>
    /// Tries to validate and normalize a character for a given raw input index.
    /// </summary>
    public bool TryNormalizeInputForRawIndex(char value, int rawIndex, out char normalized)
    {
        normalized = value;
        if (_tokens.Count == 0)
            return true;

        var token = GetInputMaskTokenAtRawIndex(rawIndex);
        if (!token.HasValue)
            return false;

        return ValidateChar(value, token.Value.Type, out normalized);
    }

    #endregion

    #region Mobile Input Processing

    /// <summary>
    /// Processes mobile text-change events by diffing old/new display text against
    /// expected state, maintaining raw text as the single source of truth.
    /// Returns the new raw text, new display text, and desired cursor position.
    /// </summary>
    public MobileInputResult ProcessMobileInput(
        string oldDisplayText,
        string newDisplayText,
        string expectedDisplayText,
        string currentRawText,
        bool showOptionalPrompts)
    {
        // 1. Our own rewrite echoed back — no-op
        if (string.Equals(newDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            return new MobileInputResult(
                currentRawText,
                expectedDisplayText,
                GetDisplayCursorPositionForRawLength(currentRawText.Length, expectedDisplayText));
        }

        // 2. Cleared
        if (string.IsNullOrEmpty(newDisplayText))
        {
            return new MobileInputResult(string.Empty, string.Empty, 0);
        }

        var maxRaw = GetMaxRawInputLength();

        // 3. Single character and old was our expected text — the IME sent just the typed char
        if (newDisplayText.Length == 1 &&
            (oldDisplayText?.Length ?? 0) > 1 &&
            string.Equals(oldDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            if (currentRawText.Length < maxRaw &&
                TryNormalizeInputForRawIndex(newDisplayText[0], currentRawText.Length, out var normalized))
            {
                var newRaw = TrimToMaskCapacity(currentRawText + normalized);
                var display = GetMaskedText(newRaw, showOptionalPrompts);
                return new MobileInputResult(
                    newRaw,
                    display,
                    GetDisplayCursorPositionForRawLength(newRaw.Length, display));
            }
            // At capacity or invalid character — keep current state
            return new MobileInputResult(
                currentRawText,
                expectedDisplayText,
                GetDisplayCursorPositionForRawLength(currentRawText.Length, expectedDisplayText));
        }

        // 4. Text got longer — user typed character(s)
        if (newDisplayText.Length > (oldDisplayText?.Length ?? 0) &&
            string.Equals(oldDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            var insertedChars = ExtractInsertedChars(oldDisplayText ?? string.Empty, newDisplayText);
            if (insertedChars.Length > 0)
            {
                var newRaw = currentRawText;
                foreach (var c in insertedChars)
                {
                    if (newRaw.Length >= maxRaw)
                        break;

                    if (TryNormalizeInputForRawIndex(c, newRaw.Length, out var normalized))
                    {
                        newRaw += normalized;
                    }
                }

                newRaw = TrimToMaskCapacity(newRaw);
                var display = GetMaskedText(newRaw, showOptionalPrompts);
                return new MobileInputResult(
                    newRaw,
                    display,
                    GetDisplayCursorPositionForRawLength(newRaw.Length, display));
            }
        }

        // 5. Text got shorter — user deleted character(s)
        if (newDisplayText.Length < (oldDisplayText?.Length ?? 0) &&
            string.Equals(oldDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            var charsRemoved = CountInputCharsRemoved(oldDisplayText ?? string.Empty, newDisplayText);
            if (charsRemoved > 0 && charsRemoved <= currentRawText.Length)
            {
                var newRaw = currentRawText[..^charsRemoved];
                var display = GetMaskedText(newRaw, showOptionalPrompts);
                var cursor = newRaw.Length > 0
                    ? GetDisplayCursorPositionForRawLength(newRaw.Length, display)
                    : 0;
                return new MobileInputResult(newRaw, display, cursor);
            }
        }

        // 6. Fallback — full re-parse
        {
            var newRaw = TrimToMaskCapacity(ExtractRawText(newDisplayText));
            var display = string.IsNullOrEmpty(newRaw)
                ? string.Empty
                : GetMaskedText(newRaw, showOptionalPrompts);
            var cursor = newRaw.Length > 0
                ? GetDisplayCursorPositionForRawLength(newRaw.Length, display)
                : 0;
            return new MobileInputResult(newRaw, display, cursor);
        }
    }

    #endregion

    #region Private Helpers

    private MaskToken? GetInputMaskTokenAtRawIndex(int rawIndex)
    {
        if (rawIndex < 0)
            return null;

        var inputIndex = 0;
        foreach (var token in _tokens)
        {
            if (token.Type == MaskTokenType.Literal)
                continue;

            if (inputIndex == rawIndex)
                return token;

            inputIndex++;
        }

        return null;
    }

    /// <summary>
    /// Finds characters that were inserted into the display text by comparing old and new.
    /// </summary>
    private static string ExtractInsertedChars(string oldText, string newText)
    {
        // Find common prefix
        var prefixLen = 0;
        while (prefixLen < oldText.Length && prefixLen < newText.Length &&
               oldText[prefixLen] == newText[prefixLen])
        {
            prefixLen++;
        }

        // The inserted portion is in the new text between the prefix and the suffix
        var suffixLen = 0;
        while (suffixLen < (oldText.Length - prefixLen) &&
               suffixLen < (newText.Length - prefixLen) &&
               oldText[^(suffixLen + 1)] == newText[^(suffixLen + 1)])
        {
            suffixLen++;
        }

        var insertedLen = newText.Length - oldText.Length;
        if (insertedLen <= 0)
            return string.Empty;

        return newText.Substring(prefixLen, insertedLen);
    }

    /// <summary>
    /// Counts how many input (non-literal, non-prompt) characters were removed
    /// by comparing old display text to new (shorter) display text.
    /// </summary>
    private int CountInputCharsRemoved(string oldDisplay, string newDisplay)
    {
        // Simple heuristic: count input chars in old minus input chars in new
        var oldInputCount = CountInputChars(oldDisplay);
        var newInputCount = CountInputChars(newDisplay);

        var diff = oldInputCount - newInputCount;
        return diff > 0 ? diff : 1; // at least 1 if text got shorter
    }

    private int CountInputChars(string displayText)
    {
        var count = 0;
        var tokenIndex = 0;

        foreach (var c in displayText)
        {
            if (tokenIndex >= _tokens.Count)
                break;

            var token = _tokens[tokenIndex];
            if (token.Type == MaskTokenType.Literal)
            {
                tokenIndex++;
                continue;
            }

            if (c != _promptChar)
                count++;

            tokenIndex++;
        }

        return count;
    }

    #endregion

    #region Inner Types

    public enum MaskTokenType
    {
        Literal,
        RequiredDigit,       // 0
        OptionalDigit,       // 9
        RequiredLetter,      // A
        OptionalLetter,      // a
        RequiredLetterUpper, // L
        OptionalLetterUpper, // ?
        RequiredAny,         // &
        OptionalAny          // C
    }

    public readonly struct MaskToken
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

    /// <summary>
    /// Result from ProcessMobileInput — new raw text, display text, and cursor position.
    /// </summary>
    public readonly struct MobileInputResult
    {
        public string RawText { get; }
        public string DisplayText { get; }
        public int CursorPosition { get; }

        public MobileInputResult(string rawText, string displayText, int cursorPosition)
        {
            RawText = rawText;
            DisplayText = displayText;
            CursorPosition = cursorPosition;
        }
    }

    #endregion
}
