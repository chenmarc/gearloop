namespace Gearloop.App.Services;

/// <summary>Small wrappers around the current page's alerts so view models stay short.</summary>
public static class Dialogs
{
    private static Page Page => Shell.Current;

    public static Task ShowAsync(string title, string message) =>
        Page.DisplayAlertAsync(title, message, "OK");

    public static Task<bool> ConfirmAsync(string title, string message, string accept) =>
        Page.DisplayAlertAsync(title, message, accept, "Cancel");

    public static Task<string?> PromptAsync(string title, string message, string initialValue = "") =>
        Page.DisplayPromptAsync(title, message, "OK", "Cancel", initialValue: initialValue, keyboard: Keyboard.Numeric);

    public static Task<string?> ChooseAsync(string title, params string[] options) =>
        Page.DisplayActionSheetAsync(title, "Cancel", null, options);
}
