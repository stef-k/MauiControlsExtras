# TokenEntry

A tag/token input control with autocomplete support.

![TokenEntry](../images/token-entry.JPG)

## Features

- **Token Display** - Displays values as removable chips/tags
- **Autocomplete** - Suggestions as you type
- **Validation** - Validate tokens before adding
- **Custom Tokens** - Allow or disallow free-text tokens
- **Keyboard Navigation** - Full keyboard support

## Basic Usage

```xml
<extras:TokenEntry
    Tokens="{Binding Tags, Mode=TwoWay}"
    Placeholder="Add tags..." />
```

## Autocomplete

```xml
<extras:TokenEntry
    Tokens="{Binding SelectedTags}"
    SuggestionsSource="{Binding AvailableTags}"
    DisplayMemberPath="Name"
    SuggestionThreshold="2" />
```

## Restrict to Suggestions

```xml
<extras:TokenEntry
    Tokens="{Binding SelectedItems}"
    SuggestionsSource="{Binding AllItems}"
    AllowFreeText="False" />
```

## Token Separators

```xml
<extras:TokenEntry
    Tokens="{Binding Tags}"
    TokenSeparators=",;|"
    CreateTokenOnSeparator="True" />
```

## Maximum Tokens

```xml
<extras:TokenEntry
    Tokens="{Binding Tags}"
    MaxTokens="5"
    MaxTokensReachedMessage="Maximum 5 tags allowed" />
```

## Token Validation

```xml
<extras:TokenEntry
    Tokens="{Binding Tags}"
    ValidateTokenCommand="{Binding ValidateTagCommand}" />
```

```csharp
public ICommand ValidateTagCommand => new Command<TokenValidationEventArgs>(e =>
{
    if (e.Token.Length < 2)
    {
        e.IsValid = false;
        e.ErrorMessage = "Tag must be at least 2 characters";
    }
});
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| Enter | Create token from current text |
| Backspace | Delete last token (when input empty) |
| Delete | Delete selected token |
| ← | Select previous token |
| → | Deselect token |

## Events

| Event | Description |
|-------|-------------|
| TokenAdded | Token was added |
| TokenRemoved | Token was removed |
| TokenValidating | Token about to be added |
| TextChanged | Input text changed |

## Commands

| Command | Description |
|---------|-------------|
| TokenAddedCommand | Execute when token added |
| TokenRemovedCommand | Execute when token removed |
| ValidateTokenCommand | Validate before adding |

## Validation

TokenEntry implements `IValidatable` for built-in validation support. This is separate from token-level validation (ValidateTokenCommand).

```xml
<extras:TokenEntry
    Tokens="{Binding Tags}"
    IsRequired="True"
    RequiredErrorMessage="Please add at least one tag"
    ValidateCommand="{Binding OnValidationCommand}" />
```

### Checking Validation State

```csharp
if (!tokenEntry.IsValid)
{
    foreach (var error in tokenEntry.ValidationErrors)
    {
        Debug.WriteLine(error);
    }
}

// Trigger validation manually
var result = tokenEntry.Validate();
```

### Validation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsRequired | bool | false | Whether at least one token is required |
| RequiredErrorMessage | string | "This field is required." | Error message when required but no tokens |
| IsValid | bool | (read-only) | Current validation state |
| ValidationErrors | IReadOnlyList&lt;string&gt; | (read-only) | List of validation error messages |
| ValidateCommand | ICommand | null | Command executed when validation occurs |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| Tokens | IList | Current tokens |
| SuggestionsSource | IEnumerable | Autocomplete suggestions |
| DisplayMemberPath | string | Property to display |
| AllowFreeText | bool | Allow non-suggestion tokens |
| MaxTokens | int | Maximum token count |
| TokenSeparators | string | Characters that create tokens |
| Placeholder | string | Input placeholder text |
