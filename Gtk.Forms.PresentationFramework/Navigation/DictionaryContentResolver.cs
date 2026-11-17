using System;
using System.Collections.Generic;

namespace System.Windows.Navigation;

/// <summary>
/// Default <see cref="IContentResolver"/>: resolves URIs from an in-process dictionary.
/// Register page instances via <see cref="NavigationService.RegisterContent"/> (or
/// directly into <see cref="Registrations"/>).
/// </summary>
/// <remarks>
/// Runtime XAML loading is not available; the markup compiler produces compiled
/// <c>InitializeComponent</c> methods instead. Apps register already-constructed
/// page objects, keyed by their pack URI (e.g. <c>new Uri("/Views/HomePage.xaml", UriKind.Relative)</c>).
/// </remarks>
public sealed class DictionaryContentResolver : IContentResolver
{
	/// <summary>URI → content object registry.</summary>
	public Dictionary<Uri, object> Registrations { get; } = new Dictionary<Uri, object>();

	/// <inheritdoc/>
	public object? Resolve(Uri uri)
	{
		if (uri == null) throw new ArgumentNullException(nameof(uri));
		Registrations.TryGetValue(uri, out var result);
		return result;
	}
}