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
			MessageBoxButton.OK          => new[] { "_OK" },
			MessageBoxButton.OKCancel    => new[] { "_OK", "_Cancel" },
			MessageBoxButton.YesNo       => new[] { "_Yes", "_No" },
			MessageBoxButton.YesNoCancel => new[] { "_Yes", "_No", "_Cancel" },
			_                            => new[] { "_OK" }
		};

	/// <summary>
	/// Maps a button label (as returned by the GTK dialog) to a
	/// <see cref="MessageBoxResult"/>. Unknown labels map to <see cref="MessageBoxResult.None"/>.
	/// </summary>
	internal static MessageBoxResult ResultFromLabel(string? label) =>
		label switch
		{
			"_OK"     or "OK"     => MessageBoxResult.OK,
			"_Cancel" or "Cancel" => MessageBoxResult.Cancel,
			"_Yes"    or "Yes"    => MessageBoxResult.Yes,
			"_No"     or "No"     => MessageBoxResult.No,
			_                     => MessageBoxResult.None
		};

	/// <summary>
	/// Maps the default button (first) in <paramref name="button"/> to the expected
	/// <see cref="MessageBoxResult"/> — used when a dialog is dismissed without a click.
	/// </summary>
	internal static MessageBoxResult DefaultResult(MessageBoxButton button) =>
		button switch
		{
			MessageBoxButton.OK          => MessageBoxResult.OK,
			MessageBoxButton.OKCancel    => MessageBoxResult.OK,
			MessageBoxButton.YesNo       => MessageBoxResult.Yes,
			MessageBoxButton.YesNoCancel => MessageBoxResult.Yes,
			_                            => MessageBoxResult.None
		};
}