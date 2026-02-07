using System.Text;

namespace MauiControlsExtras.Helpers;

/// <summary>
/// Pure mask-processing logic extracted from MaskedEntry for testability.
/// No MAUI dependencies — fully unit-testable.
/// All methods work with input-only raw text (no literals). Use
/// <see cref="InsertLiteralsIntoRaw"/> / <see cref="RemoveLiteralsFromRaw"/>
/// at the boundary when IncludeLiterals is needed.
/// </summary>
public sealed class MaskProcessor
{
    private readonly List<MaskToken> _tokens = new();
    private char _promptChar;

    public MaskProcessor(char promptChar = '_')
    {
        _promptChar = promptChar;
    }

    public char PromptChar
    {
        get => _promptChar;
        set => _promptChar = value;
    }

    public IReadOnlyList<MaskToken> Tokens => _tokens;

    #region Mask Parsing

    public void ParseMask(string? mask)
    {
        _tokens.Clear();

        if (string.IsNullOrEmpty(mask))
            return;

        var i = 0;
        while (i < mask.Length)
        {
            var c = mask[i];

            if (c == '\\' && i + 1 < mask.Length)
            {
                _tokens.Add(new MaskToken(MaskTokenType.Literal, mask[i + 1]));
                i += 2;
                continue;
            }

            switch (c)
            {
                case '0': _tokens.Add(new MaskToken(MaskTokenType.RequiredDigit, c)); break;
                case '9': _tokens.Add(new MaskToken(MaskTokenType.OptionalDigit, c)); break;
                case 'A': _tokens.Add(new MaskToken(MaskTokenType.RequiredLetter, c)); break;
                case 'a': _tokens.Add(new MaskToken(MaskTokenType.OptionalLetter, c)); break;
                case 'L': _tokens.Add(new MaskToken(MaskTokenType.RequiredLetterUpper, c)); break;
                case '?': _tokens.Add(new MaskToken(MaskTokenType.OptionalLetterUpper, c)); break;
                case '&': _tokens.Add(new MaskToken(MaskTokenType.RequiredAny, c)); break;
                case 'C': _tokens.Add(new MaskToken(MaskTokenType.OptionalAny, c)); break;
                default: _tokens.Add(new MaskToken(MaskTokenType.Literal, c)); break;
            }

            i++;
        }
    }

    #endregion

    #region Core Operations

    /// <summary>
    /// Extracts input-only raw text from a masked display string.
    /// Always returns text without literal characters.
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
                if (c == _tokens[tokenIndex].Character)
                {
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
    /// Formats input-only raw text through the mask to produce display text.
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

    public bool ValidateChar(char c, MaskTokenType tokenType, out char output)
    {
        output = c;
        return tokenType switch
        {
            MaskTokenType.RequiredDigit or MaskTokenType.OptionalDigit => char.IsDigit(c),
            MaskTokenType.RequiredLetter or MaskTokenType.OptionalLetter => char.IsLetter(c),
            MaskTokenType.RequiredLetterUpper or MaskTokenType.OptionalLetterUpper =>
                char.IsLetter(c) && ((output = char.ToUpper(c)) == output),
            MaskTokenType.RequiredAny or MaskTokenType.OptionalAny => true,
            _ => false,
        };
    }

    public bool CheckMaskComplete(string? rawText)
    {
        if (_tokens.Count == 0)
            return !string.IsNullOrEmpty(rawText);

        var text = rawText ?? string.Empty;
        var textIndex = 0;

        foreach (var token in _tokens)
        {
            if (token.Type == MaskTokenType.Literal) continue;
            if (token.IsOptional) continue;
            if (textIndex >= text.Length) return false;
            if (!ValidateChar(text[textIndex], token.Type, out _)) return false;
            textIndex++;
        }

        return true;
    }

    public int GetMaxRawInputLength()
    {
        if (_tokens.Count == 0)
            return int.MaxValue;
        return _tokens.Count(t => t.Type != MaskTokenType.Literal);
    }

    public string TrimToMaskCapacity(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;
        var max = GetMaxRawInputLength();
        return rawText.Length <= max ? rawText : rawText[..max];
    }

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

    public bool TryNormalizeInputForRawIndex(char value, int rawIndex, out char normalized)
    {
        normalized = value;
        if (_tokens.Count == 0) return true;
        var token = GetInputMaskTokenAtRawIndex(rawIndex);
        if (!token.HasValue) return false;
        return ValidateChar(value, token.Value.Type, out normalized);
    }

    #endregion

    #region Literal Insertion / Removal

    /// <summary>
    /// Inserts literal characters into input-only raw text at the mask-defined positions.
    /// E.g. for phone mask: "5551234567" → "(555) 123-4567"
    /// </summary>
    public string InsertLiteralsIntoRaw(string inputOnlyRaw)
    {
        if (_tokens.Count == 0 || string.IsNullOrEmpty(inputOnlyRaw))
            return inputOnlyRaw ?? string.Empty;

        var result = new StringBuilder();
        var inputIndex = 0;

        foreach (var token in _tokens)
        {
            if (token.Type == MaskTokenType.Literal)
            {
                // Only include literal if there's more input after this point
                if (inputIndex < inputOnlyRaw.Length)
                    result.Append(token.Character);
            }
            else if (inputIndex < inputOnlyRaw.Length)
            {
                result.Append(inputOnlyRaw[inputIndex]);
                inputIndex++;
            }
            else
            {
                break;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Removes literal characters from raw text that was produced by <see cref="InsertLiteralsIntoRaw"/>.
    /// E.g. for phone mask: "(555) 123-4567" → "5551234567"
    /// </summary>
    public string RemoveLiteralsFromRaw(string rawWithLiterals)
    {
        if (_tokens.Count == 0 || string.IsNullOrEmpty(rawWithLiterals))
            return rawWithLiterals ?? string.Empty;

        var result = new StringBuilder();
        var charIndex = 0;

        for (var tokenIndex = 0; tokenIndex < _tokens.Count && charIndex < rawWithLiterals.Length; tokenIndex++)
        {
            if (_tokens[tokenIndex].Type == MaskTokenType.Literal)
            {
                // Skip the literal char if it matches the expected one
                if (rawWithLiterals[charIndex] == _tokens[tokenIndex].Character)
                    charIndex++;
                continue;
            }

            result.Append(rawWithLiterals[charIndex]);
            charIndex++;
        }

        return result.ToString();
    }

    #endregion

    #region Mobile Input Processing

    /// <summary>
    /// Processes mobile text-change events by parsing the new display text and comparing
    /// the candidate raw text to the current raw text. This is robust against all Android
    /// IME patterns (single-char replacement, accumulated digits, formatted text, etc.).
    /// </summary>
    public MobileInputResult ProcessMobileInput(
        string oldDisplayText,
        string newDisplayText,
        string expectedDisplayText,
        string currentRawText,
        bool showOptionalPrompts)
    {
        // 1. Echo-back: our own rewrite echoed back
        if (string.Equals(newDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            return BuildResult(currentRawText, expectedDisplayText);
        }

        // 2. Cleared
        if (string.IsNullOrEmpty(newDisplayText))
        {
            return new MobileInputResult(string.Empty, string.Empty, 0);
        }

        var maxRaw = GetMaxRawInputLength();

        // 3. Single-char IME replacement: Android often replaces full text with just the typed char
        if (newDisplayText.Length == 1 &&
            !string.IsNullOrEmpty(oldDisplayText) && oldDisplayText.Length > 1 &&
            string.Equals(oldDisplayText, expectedDisplayText, StringComparison.Ordinal))
        {
            if (currentRawText.Length < maxRaw &&
                TryNormalizeInputForRawIndex(newDisplayText[0], currentRawText.Length, out var normalized))
            {
                return BuildResult(TrimToMaskCapacity(currentRawText + normalized), showOptionalPrompts);
            }
            // At capacity or invalid — keep current state
            return BuildResult(currentRawText, expectedDisplayText);
        }

        // 4. Parse the new display text to determine candidate raw text
        var candidateRaw = TrimToMaskCapacity(ExtractRawText(newDisplayText));

        // 5. If candidate is a clean extension of current raw → append
        if (candidateRaw.Length > currentRawText.Length &&
            candidateRaw.StartsWith(currentRawText, StringComparison.Ordinal))
        {
            return BuildResult(candidateRaw, showOptionalPrompts);
        }

        // 6. If current raw starts with candidate → clean delete
        if (candidateRaw.Length < currentRawText.Length &&
            currentRawText.StartsWith(candidateRaw, StringComparison.Ordinal))
        {
            return BuildResult(candidateRaw, showOptionalPrompts);
        }

        // 7. IME may have sent accumulated raw digits without mask formatting
        //    (e.g. "55" instead of "(55_) ___-____"). Try parsing as direct raw input.
        if (newDisplayText.Length <= maxRaw)
        {
            var builtRaw = new StringBuilder();
            var allValid = true;
            for (var i = 0; i < newDisplayText.Length; i++)
            {
                if (TryNormalizeInputForRawIndex(newDisplayText[i], i, out var norm))
                    builtRaw.Append(norm);
                else
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid && builtRaw.Length > 0)
            {
                var directRaw = TrimToMaskCapacity(builtRaw.ToString());
                // Prefer this if it extends or equals current raw
                if (directRaw.Length >= currentRawText.Length)
                    return BuildResult(directRaw, showOptionalPrompts);
            }
        }

        // 8. Fallback — use candidate raw as-is
        if (!string.IsNullOrEmpty(candidateRaw))
        {
            return BuildResult(candidateRaw, showOptionalPrompts);
        }

        // 9. Nothing valid extracted — keep current
        return BuildResult(currentRawText, expectedDisplayText);
    }

    private MobileInputResult BuildResult(string rawText, bool showOptionalPrompts)
    {
        var display = string.IsNullOrEmpty(rawText)
            ? string.Empty
            : GetMaskedText(rawText, showOptionalPrompts);
        return BuildResult(rawText, display);
    }

    private MobileInputResult BuildResult(string rawText, string displayText)
    {
        var cursor = rawText.Length > 0
            ? GetDisplayCursorPositionForRawLength(rawText.Length, displayText)
            : 0;
        return new MobileInputResult(rawText, displayText, cursor);
    }

    #endregion

    #region Private Helpers

    private MaskToken? GetInputMaskTokenAtRawIndex(int rawIndex)
    {
        if (rawIndex < 0) return null;
        var inputIndex = 0;
        foreach (var token in _tokens)
        {
            if (token.Type == MaskTokenType.Literal) continue;
            if (inputIndex == rawIndex) return token;
            inputIndex++;
        }
        return null;
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
