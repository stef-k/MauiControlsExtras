using MauiControlsExtras.Helpers;
using static MauiControlsExtras.Helpers.MaskProcessor;

namespace MauiControlsExtras.Tests.Helpers;

public class MaskProcessorTests
{
    private MaskProcessor CreateProcessor(string mask, char promptChar = '_')
    {
        var p = new MaskProcessor(promptChar);
        p.ParseMask(mask);
        return p;
    }

    #region ParseMask

    [Fact]
    public void ParseMask_PhoneUS_ProducesCorrectTokens()
    {
        var p = CreateProcessor("(000) 000-0000");

        // (  0  0  0  )     0  0  0  -  0  0  0  0  = 14 tokens
        Assert.Equal(14, p.Tokens.Count);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[0].Type);    // (
        Assert.Equal(MaskTokenType.RequiredDigit, p.Tokens[1].Type);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[4].Type);    // )
        Assert.Equal(MaskTokenType.Literal, p.Tokens[5].Type);    // space
        Assert.Equal(MaskTokenType.Literal, p.Tokens[9].Type);    // -
    }

    [Fact]
    public void ParseMask_DateUS_ProducesCorrectTokens()
    {
        var p = CreateProcessor("00/00/0000");

        Assert.Equal(10, p.Tokens.Count);
        Assert.Equal(MaskTokenType.RequiredDigit, p.Tokens[0].Type);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[2].Type); // /
    }

    [Fact]
    public void ParseMask_SSN_ProducesCorrectTokens()
    {
        var p = CreateProcessor("000-00-0000");

        Assert.Equal(11, p.Tokens.Count);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[3].Type); // -
        Assert.Equal(MaskTokenType.Literal, p.Tokens[6].Type); // -
    }

    [Fact]
    public void ParseMask_CreditCard_ProducesCorrectTokens()
    {
        var p = CreateProcessor("0000 0000 0000 0000");

        Assert.Equal(19, p.Tokens.Count);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[4].Type); // space
    }

    [Fact]
    public void ParseMask_ZipUS_HasOptionalDigits()
    {
        var p = CreateProcessor("00000-9999");

        Assert.Equal(10, p.Tokens.Count);
        Assert.Equal(MaskTokenType.RequiredDigit, p.Tokens[0].Type);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[5].Type); // -
        Assert.Equal(MaskTokenType.OptionalDigit, p.Tokens[6].Type);
        Assert.True(p.Tokens[6].IsOptional);
    }

    [Fact]
    public void ParseMask_EscapeCharacter_CreatesLiteral()
    {
        var p = CreateProcessor(@"\0abc");

        // \0 → literal '0', then a, b, c
        Assert.Equal(4, p.Tokens.Count);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[0].Type);
        Assert.Equal('0', p.Tokens[0].Character);
        Assert.Equal(MaskTokenType.OptionalLetter, p.Tokens[1].Type);
    }

    [Fact]
    public void ParseMask_AllTokenTypes()
    {
        var p = CreateProcessor("09Aa L?&C");

        Assert.Equal(MaskTokenType.RequiredDigit, p.Tokens[0].Type);
        Assert.Equal(MaskTokenType.OptionalDigit, p.Tokens[1].Type);
        Assert.Equal(MaskTokenType.RequiredLetter, p.Tokens[2].Type);
        Assert.Equal(MaskTokenType.OptionalLetter, p.Tokens[3].Type);
        Assert.Equal(MaskTokenType.Literal, p.Tokens[4].Type);       // space
        Assert.Equal(MaskTokenType.RequiredLetterUpper, p.Tokens[5].Type);
        Assert.Equal(MaskTokenType.OptionalLetterUpper, p.Tokens[6].Type);
        Assert.Equal(MaskTokenType.RequiredAny, p.Tokens[7].Type);
        Assert.Equal(MaskTokenType.OptionalAny, p.Tokens[8].Type);
    }

    [Fact]
    public void ParseMask_NullOrEmpty_ProducesNoTokens()
    {
        var p1 = CreateProcessor(null!);
        var p2 = CreateProcessor("");

        Assert.Empty(p1.Tokens);
        Assert.Empty(p2.Tokens);
    }

    #endregion

    #region ExtractRawText

    [Fact]
    public void ExtractRawText_PhoneMask_ExtractsDigits()
    {
        var p = CreateProcessor("(000) 000-0000");

        var raw = p.ExtractRawText("(555) 123-4567");

        Assert.Equal("5551234567", raw);
    }

    [Fact]
    public void ExtractRawText_PartialInput()
    {
        var p = CreateProcessor("(000) 000-0000");

        var raw = p.ExtractRawText("(55_) ___-____");

        Assert.Equal("55", raw);
    }

    [Fact]
    public void ExtractRawText_Empty_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal(string.Empty, p.ExtractRawText(""));
        Assert.Equal(string.Empty, p.ExtractRawText(null));
    }

    [Fact]
    public void ExtractRawText_SingleDigit()
    {
        var p = CreateProcessor("(000) 000-0000");

        var raw = p.ExtractRawText("(5__) ___-____");

        Assert.Equal("5", raw);
    }

    [Fact]
    public void ExtractRawText_OverCapacity_Trimmed()
    {
        var p = CreateProcessor("(000) 000-0000");

        // 10 digits is max, try with extra
        var raw = p.ExtractRawText("(555) 123-456789");

        Assert.Equal(10, raw.Length);
    }

    [Fact]
    public void ExtractRawText_GarbledInput_BestEffort()
    {
        var p = CreateProcessor("(000) 000-0000");

        // Non-digit chars should be skipped
        var raw = p.ExtractRawText("abc123");

        Assert.Equal("123", raw);
    }

    [Fact]
    public void ExtractRawText_NoMask_ReturnsInput()
    {
        var p = CreateProcessor("");

        Assert.Equal("hello", p.ExtractRawText("hello"));
    }

    #endregion

    #region GetMaskedText

    [Fact]
    public void GetMaskedText_FullPhoneNumber()
    {
        var p = CreateProcessor("(000) 000-0000");

        var masked = p.GetMaskedText("5551234567", showOptionalPrompts: true);

        Assert.Equal("(555) 123-4567", masked);
    }

    [Fact]
    public void GetMaskedText_PartialFill_ShowPrompts()
    {
        var p = CreateProcessor("(000) 000-0000");

        var masked = p.GetMaskedText("555", showOptionalPrompts: true);

        Assert.Equal("(555) ___-____", masked);
    }

    [Fact]
    public void GetMaskedText_PartialFill_HideOptional()
    {
        var p = CreateProcessor("00000-9999");

        // 5 required digits filled, 4 optional not
        var masked = p.GetMaskedText("12345", showOptionalPrompts: false);

        Assert.Equal("12345-", masked);
    }

    [Fact]
    public void GetMaskedText_PartialFill_ShowOptional()
    {
        var p = CreateProcessor("00000-9999");

        var masked = p.GetMaskedText("12345", showOptionalPrompts: true);

        Assert.Equal("12345-____", masked);
    }

    [Fact]
    public void GetMaskedText_Empty_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal(string.Empty, p.GetMaskedText("", showOptionalPrompts: true));
        Assert.Equal(string.Empty, p.GetMaskedText(null, showOptionalPrompts: true));
    }

    [Fact]
    public void GetMaskedText_NoMask_ReturnsInput()
    {
        var p = CreateProcessor("");

        Assert.Equal("hello", p.GetMaskedText("hello", showOptionalPrompts: false));
    }

    [Fact]
    public void GetMaskedText_UppercaseMask()
    {
        var p = CreateProcessor("LL-0000");

        var masked = p.GetMaskedText("ab1234", showOptionalPrompts: false);

        Assert.Equal("AB-1234", masked);
    }

    #endregion

    #region ValidateChar

    [Fact]
    public void ValidateChar_Digit_AcceptsDigits()
    {
        var p = CreateProcessor("0");

        Assert.True(p.ValidateChar('5', MaskTokenType.RequiredDigit, out _));
        Assert.False(p.ValidateChar('a', MaskTokenType.RequiredDigit, out _));
    }

    [Fact]
    public void ValidateChar_Letter_AcceptsLetters()
    {
        var p = CreateProcessor("A");

        Assert.True(p.ValidateChar('x', MaskTokenType.RequiredLetter, out _));
        Assert.False(p.ValidateChar('5', MaskTokenType.RequiredLetter, out _));
    }

    [Fact]
    public void ValidateChar_UppercaseLetter_Normalizes()
    {
        var p = CreateProcessor("L");

        Assert.True(p.ValidateChar('a', MaskTokenType.RequiredLetterUpper, out var output));
        Assert.Equal('A', output);
    }

    [Fact]
    public void ValidateChar_Any_AcceptsAll()
    {
        var p = CreateProcessor("&");

        Assert.True(p.ValidateChar('x', MaskTokenType.RequiredAny, out _));
        Assert.True(p.ValidateChar('5', MaskTokenType.RequiredAny, out _));
        Assert.True(p.ValidateChar('@', MaskTokenType.RequiredAny, out _));
    }

    #endregion

    #region CheckMaskComplete

    [Fact]
    public void CheckMaskComplete_FullyFilled_True()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.True(p.CheckMaskComplete("5551234567"));
    }

    [Fact]
    public void CheckMaskComplete_Partial_False()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.False(p.CheckMaskComplete("555"));
    }

    [Fact]
    public void CheckMaskComplete_Empty_False()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.False(p.CheckMaskComplete(""));
        Assert.False(p.CheckMaskComplete(null));
    }

    [Fact]
    public void CheckMaskComplete_OptionalOnly_AlwaysTrue()
    {
        var p = CreateProcessor("9999");

        // All optional — even empty should be complete
        Assert.True(p.CheckMaskComplete(""));
    }

    [Fact]
    public void CheckMaskComplete_MixedRequiredOptional()
    {
        var p = CreateProcessor("00000-9999");

        // 5 required digits filled, optional not
        Assert.True(p.CheckMaskComplete("12345"));
    }

    [Fact]
    public void CheckMaskComplete_NoMask_NonEmpty_True()
    {
        var p = CreateProcessor("");

        Assert.True(p.CheckMaskComplete("hello"));
        Assert.False(p.CheckMaskComplete(""));
    }

    #endregion

    #region GetMaxRawInputLength

    [Fact]
    public void GetMaxRawInputLength_PhoneMask_10()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal(10, p.GetMaxRawInputLength());
    }

    [Fact]
    public void GetMaxRawInputLength_NoMask_IntMax()
    {
        var p = CreateProcessor("");

        Assert.Equal(int.MaxValue, p.GetMaxRawInputLength());
    }

    [Fact]
    public void GetMaxRawInputLength_SSN_9()
    {
        var p = CreateProcessor("000-00-0000");

        Assert.Equal(9, p.GetMaxRawInputLength());
    }

    #endregion

    #region TrimToMaskCapacity

    [Fact]
    public void TrimToMaskCapacity_UnderLimit_NoChange()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal("555", p.TrimToMaskCapacity("555"));
    }

    [Fact]
    public void TrimToMaskCapacity_OverLimit_Trims()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal("5551234567", p.TrimToMaskCapacity("55512345678"));
    }

    [Fact]
    public void TrimToMaskCapacity_Empty_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal("", p.TrimToMaskCapacity(""));
    }

    #endregion

    #region GetDisplayCursorPositionForRawLength

    [Fact]
    public void CursorPosition_PhoneMask_AfterAreaCode()
    {
        var p = CreateProcessor("(000) 000-0000");
        var display = p.GetMaskedText("555", showOptionalPrompts: true);

        // After 3 raw chars "(555) ___-____" → cursor at position 6 (after ") ")
        var cursor = p.GetDisplayCursorPositionForRawLength(3, display);

        Assert.Equal(6, cursor);
    }

    [Fact]
    public void CursorPosition_PhoneMask_AtStart()
    {
        var p = CreateProcessor("(000) 000-0000");
        // With actual content, GetMaskedText produces "(___) ___-____"
        var display = p.GetMaskedText("5", showOptionalPrompts: true);

        // After 0 raw chars — cursor at position 1 (after "(")
        var cursor = p.GetDisplayCursorPositionForRawLength(0, display);

        Assert.Equal(1, cursor);
    }

    [Fact]
    public void CursorPosition_PhoneMask_Complete()
    {
        var p = CreateProcessor("(000) 000-0000");
        var display = p.GetMaskedText("5551234567", showOptionalPrompts: true);

        var cursor = p.GetDisplayCursorPositionForRawLength(10, display);

        Assert.Equal(14, cursor); // end of "(555) 123-4567"
    }

    [Fact]
    public void CursorPosition_NoMask_EqualsRawLength()
    {
        var p = CreateProcessor("");

        var cursor = p.GetDisplayCursorPositionForRawLength(5, "hello");

        Assert.Equal(5, cursor);
    }

    [Fact]
    public void CursorPosition_SSN_AfterFirstGroup()
    {
        var p = CreateProcessor("000-00-0000");
        var display = p.GetMaskedText("123", showOptionalPrompts: true);

        // "123-__-____" → cursor at position 4 (after "123-")
        var cursor = p.GetDisplayCursorPositionForRawLength(3, display);

        Assert.Equal(4, cursor);
    }

    #endregion

    #region TryNormalizeInputForRawIndex

    [Fact]
    public void TryNormalize_ValidDigit_True()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.True(p.TryNormalizeInputForRawIndex('5', 0, out var normalized));
        Assert.Equal('5', normalized);
    }

    [Fact]
    public void TryNormalize_InvalidChar_False()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.False(p.TryNormalizeInputForRawIndex('a', 0, out _));
    }

    [Fact]
    public void TryNormalize_BeyondMask_False()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.False(p.TryNormalizeInputForRawIndex('5', 99, out _));
    }

    [Fact]
    public void TryNormalize_NoMask_AlwaysTrue()
    {
        var p = CreateProcessor("");

        Assert.True(p.TryNormalizeInputForRawIndex('x', 0, out _));
    }

    #endregion

    #region ProcessMobileInput

    [Fact]
    public void Mobile_EchoBack_NoOp()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.ProcessMobileInput(
            oldDisplayText: "(5__) ___-____",
            newDisplayText: "(5__) ___-____",
            expectedDisplayText: "(5__) ___-____",
            currentRawText: "5",
            showOptionalPrompts: true);

        Assert.Equal("5", result.RawText);
        Assert.Equal("(5__) ___-____", result.DisplayText);
    }

    [Fact]
    public void Mobile_Clear_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.ProcessMobileInput(
            oldDisplayText: "(5__) ___-____",
            newDisplayText: "",
            expectedDisplayText: "(5__) ___-____",
            currentRawText: "5",
            showOptionalPrompts: true);

        Assert.Equal(string.Empty, result.RawText);
        Assert.Equal(string.Empty, result.DisplayText);
        Assert.Equal(0, result.CursorPosition);
    }

    [Fact]
    public void Mobile_SingleCharReplacement_AppendsToRaw()
    {
        var p = CreateProcessor("(000) 000-0000");

        // IME sends just "5" instead of full masked text — the Android single-char pattern
        var result = p.ProcessMobileInput(
            oldDisplayText: "(___) ___-____",
            newDisplayText: "5",
            expectedDisplayText: "(___) ___-____",
            currentRawText: "",
            showOptionalPrompts: true);

        Assert.Equal("5", result.RawText);
        Assert.Equal("(5__) ___-____", result.DisplayText);
    }

    [Fact]
    public void Mobile_SingleCharReplacement_InvalidChar_KeepsCurrent()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.ProcessMobileInput(
            oldDisplayText: "(___) ___-____",
            newDisplayText: "a",
            expectedDisplayText: "(___) ___-____",
            currentRawText: "",
            showOptionalPrompts: true);

        Assert.Equal("", result.RawText);
        Assert.Equal("(___) ___-____", result.DisplayText);
    }

    [Fact]
    public void Mobile_TextGotLonger_AppendsTypedChar()
    {
        var p = CreateProcessor("(000) 000-0000");
        var currentDisplay = "(55_) ___-____";

        // User typed '5' at the end — IME sends old text + new char
        var result = p.ProcessMobileInput(
            oldDisplayText: currentDisplay,
            newDisplayText: "(555) ___-____",
            expectedDisplayText: currentDisplay,
            currentRawText: "55",
            showOptionalPrompts: true);

        Assert.Equal("555", result.RawText);
        Assert.StartsWith("(555)", result.DisplayText);
    }

    [Fact]
    public void Mobile_TextGotShorter_TrimsRaw()
    {
        var p = CreateProcessor("(000) 000-0000");
        var currentDisplay = "(555) ___-____";

        // User deleted one character
        var result = p.ProcessMobileInput(
            oldDisplayText: currentDisplay,
            newDisplayText: "(55_) ___-____",
            expectedDisplayText: currentDisplay,
            currentRawText: "555",
            showOptionalPrompts: true);

        Assert.Equal("55", result.RawText);
    }

    [Fact]
    public void Mobile_Fallback_FullReparse()
    {
        var p = CreateProcessor("(000) 000-0000");

        // Old doesn't match expected — fallback re-parse
        var result = p.ProcessMobileInput(
            oldDisplayText: "something unexpected",
            newDisplayText: "(555) 123-4567",
            expectedDisplayText: "(5__) ___-____",
            currentRawText: "5",
            showOptionalPrompts: true);

        Assert.Equal("5551234567", result.RawText);
        Assert.Equal("(555) 123-4567", result.DisplayText);
    }

    #endregion

    #region Integration: Sequential Phone Typing

    [Fact]
    public void Integration_PhoneMask_SequentialTyping()
    {
        var p = CreateProcessor("(000) 000-0000");
        var rawText = "";
        var expectedDisplay = "";
        var digits = "5551234567";

        foreach (var digit in digits)
        {
            // Simulate mobile IME sending single-char replacement
            var result = p.ProcessMobileInput(
                oldDisplayText: expectedDisplay.Length > 1 ? expectedDisplay : "",
                newDisplayText: digit.ToString(),
                expectedDisplayText: expectedDisplay,
                currentRawText: rawText,
                showOptionalPrompts: true);

            rawText = result.RawText;
            expectedDisplay = result.DisplayText;

            // Cursor should be beyond the just-typed character
            Assert.True(result.CursorPosition > 0);
        }

        Assert.Equal("5551234567", rawText);
        Assert.Equal("(555) 123-4567", expectedDisplay);
    }

    [Fact]
    public void Integration_PhoneMask_AppendStyleTyping()
    {
        var p = CreateProcessor("(000) 000-0000");
        var rawText = "";
        var expectedDisplay = p.GetMaskedText("", showOptionalPrompts: true);
        var digits = "5551234567";

        foreach (var digit in digits)
        {
            // Simulate "text got longer" IME pattern
            var oldDisplay = expectedDisplay;
            var tentativeRaw = rawText + digit;
            var tentativeDisplay = p.GetMaskedText(tentativeRaw, showOptionalPrompts: true);

            var result = p.ProcessMobileInput(
                oldDisplayText: oldDisplay,
                newDisplayText: tentativeDisplay,
                expectedDisplayText: oldDisplay,
                currentRawText: rawText,
                showOptionalPrompts: true);

            rawText = result.RawText;
            expectedDisplay = result.DisplayText;
        }

        Assert.Equal("5551234567", rawText);
        Assert.Equal("(555) 123-4567", expectedDisplay);
    }

    [Fact]
    public void Integration_PhoneMask_BackspaceToEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");
        var rawText = "555";
        var expectedDisplay = p.GetMaskedText(rawText, showOptionalPrompts: true);

        // Delete 3 characters one at a time
        for (var i = 0; i < 3; i++)
        {
            var oldDisplay = expectedDisplay;
            var newRaw = rawText.Length > 0 ? rawText[..^1] : "";
            var newDisplay = newRaw.Length > 0
                ? p.GetMaskedText(newRaw, showOptionalPrompts: true)
                : "";

            var result = p.ProcessMobileInput(
                oldDisplayText: oldDisplay,
                newDisplayText: newDisplay,
                expectedDisplayText: oldDisplay,
                currentRawText: rawText,
                showOptionalPrompts: true);

            rawText = result.RawText;
            expectedDisplay = result.DisplayText;
        }

        Assert.Equal("", rawText);
    }

    #endregion

    #region Integration: SSN

    [Fact]
    public void Integration_SSN_FullEntry()
    {
        var p = CreateProcessor("000-00-0000");

        var masked = p.GetMaskedText("123456789", showOptionalPrompts: false);
        Assert.Equal("123-45-6789", masked);
        Assert.True(p.CheckMaskComplete("123456789"));

        var raw = p.ExtractRawText("123-45-6789");
        Assert.Equal("123456789", raw);
    }

    #endregion

    #region Integration: Canadian Postal Code

    [Fact]
    public void Integration_CanadianPostalCode()
    {
        // A0A 0A0 → RequiredLetter, RequiredDigit, RequiredLetter, space, RequiredDigit, RequiredLetter, RequiredDigit
        var p = CreateProcessor("A0A 0A0");

        var masked = p.GetMaskedText("K1A0B1", showOptionalPrompts: false);
        Assert.Equal("K1A 0B1", masked);
        Assert.True(p.CheckMaskComplete("K1A0B1"));
    }

    #endregion

    #region Integration: Date/Time

    [Fact]
    public void Integration_DateISO()
    {
        var p = CreateProcessor("0000-00-00");

        var masked = p.GetMaskedText("20260206", showOptionalPrompts: false);
        Assert.Equal("2026-02-06", masked);
        Assert.True(p.CheckMaskComplete("20260206"));
    }

    [Fact]
    public void Integration_TimeHHMMSS()
    {
        var p = CreateProcessor("00:00:00");

        var masked = p.GetMaskedText("143022", showOptionalPrompts: false);
        Assert.Equal("14:30:22", masked);
    }

    #endregion

    #region Integration: Credit Card

    [Fact]
    public void Integration_CreditCard()
    {
        var p = CreateProcessor("0000 0000 0000 0000");

        var masked = p.GetMaskedText("4111111111111111", showOptionalPrompts: false);
        Assert.Equal("4111 1111 1111 1111", masked);
        Assert.True(p.CheckMaskComplete("4111111111111111"));
    }

    #endregion

    #region InsertLiteralsIntoRaw

    [Fact]
    public void InsertLiterals_PhoneMask_InsertsParensDashSpace()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.InsertLiteralsIntoRaw("5551234567");

        Assert.Equal("(555) 123-4567", result);
    }

    [Fact]
    public void InsertLiterals_PartialInput_InsertsUpToInput()
    {
        var p = CreateProcessor("(000) 000-0000");

        // Trailing literals after last input char are not included
        var result = p.InsertLiteralsIntoRaw("555");

        Assert.Equal("(555", result);
    }

    [Fact]
    public void InsertLiterals_Empty_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal(string.Empty, p.InsertLiteralsIntoRaw(""));
        Assert.Equal(string.Empty, p.InsertLiteralsIntoRaw(null!));
    }

    [Fact]
    public void InsertLiterals_IPMask_InsertsDots()
    {
        var p = CreateProcessor("099.099.099.099");

        var result = p.InsertLiteralsIntoRaw("192168001001");

        Assert.Equal("192.168.001.001", result);
    }

    [Fact]
    public void InsertLiterals_SSN_InsertsDashes()
    {
        var p = CreateProcessor("000-00-0000");

        var result = p.InsertLiteralsIntoRaw("123456789");

        Assert.Equal("123-45-6789", result);
    }

    #endregion

    #region RemoveLiteralsFromRaw

    [Fact]
    public void RemoveLiterals_PhoneMask_StripsParensDashSpace()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.RemoveLiteralsFromRaw("(555) 123-4567");

        Assert.Equal("5551234567", result);
    }

    [Fact]
    public void RemoveLiterals_PartialInput()
    {
        var p = CreateProcessor("(000) 000-0000");

        var result = p.RemoveLiteralsFromRaw("(555)");

        Assert.Equal("555", result);
    }

    [Fact]
    public void RemoveLiterals_Empty_ReturnsEmpty()
    {
        var p = CreateProcessor("(000) 000-0000");

        Assert.Equal(string.Empty, p.RemoveLiteralsFromRaw(""));
        Assert.Equal(string.Empty, p.RemoveLiteralsFromRaw(null!));
    }

    [Fact]
    public void RemoveLiterals_IPMask_StripsDots()
    {
        var p = CreateProcessor("099.099.099.099");

        var result = p.RemoveLiteralsFromRaw("192.168.001.001");

        Assert.Equal("192168001001", result);
    }

    [Fact]
    public void RemoveLiterals_Roundtrip_WithInsert()
    {
        var p = CreateProcessor("(000) 000-0000");
        var inputOnly = "5551234567";

        var withLiterals = p.InsertLiteralsIntoRaw(inputOnly);
        var stripped = p.RemoveLiteralsFromRaw(withLiterals);

        Assert.Equal(inputOnly, stripped);
    }

    #endregion

    #region Mobile: Accumulated Digits IME Pattern

    [Fact]
    public void Mobile_AccumulatedDigits_PhoneMask()
    {
        var p = CreateProcessor("(000) 000-0000");

        // Android IME sends "55" (raw digits) instead of formatted "(55_) ___-____"
        var result = p.ProcessMobileInput(
            oldDisplayText: "(5__) ___-____",
            newDisplayText: "55",
            expectedDisplayText: "(5__) ___-____",
            currentRawText: "5",
            showOptionalPrompts: true);

        // Should append to get "55", not delete
        Assert.Equal("55", result.RawText);
        Assert.Equal("(55_) ___-____", result.DisplayText);
    }

    [Fact]
    public void Mobile_AccumulatedDigits_ThreeDigits()
    {
        var p = CreateProcessor("(000) 000-0000");

        // IME sends "555" as accumulated digits
        var result = p.ProcessMobileInput(
            oldDisplayText: "(55_) ___-____",
            newDisplayText: "555",
            expectedDisplayText: "(55_) ___-____",
            currentRawText: "55",
            showOptionalPrompts: true);

        Assert.Equal("555", result.RawText);
        Assert.Equal("(555) ___-____", result.DisplayText);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ProcessMobileInput_AtCapacity_NoOverflow()
    {
        var p = CreateProcessor("(000) 000-0000");
        var fullRaw = "5551234567";
        var fullDisplay = p.GetMaskedText(fullRaw, showOptionalPrompts: true);

        // Try to type another digit at capacity
        var result = p.ProcessMobileInput(
            oldDisplayText: fullDisplay,
            newDisplayText: "8",
            expectedDisplayText: fullDisplay,
            currentRawText: fullRaw,
            showOptionalPrompts: true);

        // Should not exceed capacity
        Assert.Equal(10, result.RawText.Length);
    }

    [Fact]
    public void CustomPromptChar_WorksCorrectly()
    {
        var p = new MaskProcessor('#');
        p.ParseMask("(000) 000-0000");

        var masked = p.GetMaskedText("555", showOptionalPrompts: true);

        Assert.Equal("(555) ###-####", masked);
    }

    [Fact]
    public void Roundtrip_ExtractThenMask_Consistent()
    {
        var p = CreateProcessor("(000) 000-0000");

        var original = "(555) 123-4567";
        var raw = p.ExtractRawText(original);
        var masked = p.GetMaskedText(raw, showOptionalPrompts: false);

        Assert.Equal(original, masked);
    }

    #endregion
}
