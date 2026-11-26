using System;

namespace System.Windows.Navigation;

/// <summary>
/// Pluggable resolver that maps a <see cref="Uri"/> to a navigation-target object
/// (typically a <see cref="System.Windows.Controls.Page"/> or any <see cref="UIElement"/>).
/// </summary>
/// <remarks>
/// The default implementation is <see cref="DictionaryContentResolver"/>, which looks
/// up the URI in a caller-registered dictionary. A runtime XAML loader is not available
/// (pages are compiled by the markup compiler) — register page instances with
/// <see cref="NavigationService.RegisterContent"/> instead.
/// </remarks>
public interface IContentResolver
{
	/// <summary>
	/// Resolves <paramref name="uri"/> to a navigation target. Returns <c>null</c>
	/// when the URI is not recognised; <see cref="NavigationService"/> will then
	/// navigate to <c>null</c> content.
	/// </summary>
	object? Resolve(Uri uri);
}