using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Pure, GTK-free logic that maps a <see cref="MessageBoxButton"/> to the set of
/// response labels and maps a clicked label back to a <see cref="MessageBoxResult"/>.
/// </summary>
/// <remarks>
/// Separated from the GTK presenter so it can be unit-tested without a display.
/// </remarks>
internal static class MessageBoxButtonMapping
{
	/// <summary>Returns the ordered list of button labels for <paramref name="button"/>.</summary>
	internal static IReadOnlyList<string> ButtonLabels(MessageBoxButton button) =>
		button switch
		{
			MessageBoxButton.OK                => new[] { "_OK" },
			MessageBoxButton.OKCancel          => new[] { "_OK", "_Cancel" },
			MessageBoxButton.AbortRetryIgnore  => new[] { "_Abort", "_Retry", "_Ignore" },
			MessageBoxButton.YesNoCancel       => new[] { "_Yes", "_No", "_Cancel" },
			MessageBoxButton.YesNo             => new[] { "_Yes", "_No" },
			MessageBoxButton.RetryCancel       => new[] { "_Retry", "_Cancel" },
			MessageBoxButton.CancelTryContinue => new[] { "_Cancel", "_Try Again", "_Continue" },
			_                                  => new[] { "_OK" }
		};

	/// <summary>
	/// Maps a button label (as returned by the GTK dialog) to a
	/// <see cref="MessageBoxResult"/>. Unknown labels map to <see cref="MessageBoxResult.None"/>.
	/// </summary>
	internal static MessageBoxResult ResultFromLabel(string? label) =>
		label switch
		{
			"_OK"        or "OK"        => MessageBoxResult.OK,
			"_Cancel"    or "Cancel"    => MessageBoxResult.Cancel,
			"_Abort"     or "Abort"     => MessageBoxResult.Abort,
			"_Retry"     or "Retry"     => MessageBoxResult.Retry,
			"_Ignore"    or "Ignore"    => MessageBoxResult.Ignore,
			"_Yes"       or "Yes"       => MessageBoxResult.Yes,
			"_No"        or "No"        => MessageBoxResult.No,
			"_Try Again" or "Try Again" => MessageBoxResult.TryAgain,
			"_Continue"  or "Continue"  => MessageBoxResult.Continue,
			_                           => MessageBoxResult.None
		};

	/// <summary>
	/// The result of dismissing the dialog without a click, with Win32 semantics: Cancel when the
	/// set has a Cancel button, OK for the OK-only set, otherwise <see cref="MessageBoxResult.None"/>
	/// (Win32 does not allow dismissing Yes/No or Abort/Retry/Ignore).
	/// </summary>
	internal static MessageBoxResult DefaultResult(MessageBoxButton button) =>
		button switch
		{
			MessageBoxButton.OK                => MessageBoxResult.OK,
			MessageBoxButton.OKCancel          => MessageBoxResult.Cancel,
			MessageBoxButton.YesNoCancel       => MessageBoxResult.Cancel,
			MessageBoxButton.RetryCancel       => MessageBoxResult.Cancel,
			MessageBoxButton.CancelTryContinue => MessageBoxResult.Cancel,
			_                                  => MessageBoxResult.None
		};
}
