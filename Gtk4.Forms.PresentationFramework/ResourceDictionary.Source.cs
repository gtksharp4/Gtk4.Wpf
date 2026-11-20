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
			var assembly = Application.Current?.GetType().Assembly ?? Assembly.GetEntryAssembly();
			var loaded = GtkFormsXamlRegistry.TryCreate(value, assembly)
				?? throw new IOException("Cannot locate resource '" + value.OriginalString + "'.");
			if (loaded is not ResourceDictionary dictionary)
				throw new InvalidOperationException(
					"'" + value.OriginalString + "' does not have a ResourceDictionary as its root element.");
			_source = value;
			Clear();
			MergedDictionaries.Clear();
			foreach (var pair in dictionary) this[pair.Key] = pair.Value;
			foreach (var merged in dictionary.MergedDictionaries) MergedDictionaries.Add(merged);
		}
	}
}
