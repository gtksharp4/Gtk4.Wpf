using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows;

public partial class ResourceDictionary
{
	Uri? _source;

	/// <summary>
	/// Gets or sets the location of a compiled resource-dictionary XAML. Setting it replaces the local
	/// entries with those of the dictionary registered for the URI (<see cref="GtkFormsXamlRegistry"/>).
	/// </summary>
	/// <exception cref="ArgumentException">The URI is <see langword="null"/> or empty.</exception>
	/// <exception cref="IOException">No dictionary is registered for the URI.</exception>
	/// <exception cref="InvalidOperationException">The registered element is no <see cref="ResourceDictionary"/>.</exception>
	public Uri Source
	{
		get => _source!;
		set
		{
			if (value == null || string.IsNullOrEmpty(value.OriginalString))
				throw new ArgumentException("The string cannot be empty.", nameof(value));
			var loaded = Locate(value)
				?? throw new IOException("Cannot locate resource '" + value.OriginalString + "'.");
			if (loaded is not ResourceDictionary dictionary)
				throw new InvalidOperationException(
					"'" + value.OriginalString + "' does not have a ResourceDictionary as its root element.");
			Apply(value, dictionary);
		}
	}

	// Non-throwing variant for generated code: false when nothing is registered or it is no dictionary.
	internal bool TrySetSource(Uri value)
	{
		if (value == null || string.IsNullOrEmpty(value.OriginalString)) return false;
		if (Locate(value) is not ResourceDictionary dictionary) return false;
		Apply(value, dictionary);
		return true;
	}

	static object? Locate(Uri value) =>
		System.Windows.Themes.FluentTheme.TryCreate(value)
		?? GtkFormsXamlRegistry.TryCreate(value, Application.Current?.GetType().Assembly ?? Assembly.GetEntryAssembly());

	void Apply(Uri value, ResourceDictionary dictionary)
	{
		_source = value;
		Clear();
		MergedDictionaries.Clear();
		CopyEntriesFrom(dictionary);
	}
}
