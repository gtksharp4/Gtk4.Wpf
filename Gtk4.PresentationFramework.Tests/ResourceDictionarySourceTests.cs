using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary><see cref="ResourceDictionary.Source"/> loads a registered compiled dictionary at runtime.</summary>
public class ResourceDictionarySourceTests
{
	static Uri Rel(string path) => new Uri(path, UriKind.RelativeOrAbsolute);

	[Fact]
	public void Source_loads_the_entries_and_merged_dictionaries_of_the_registered_dictionary()
	{
		var inner = new ResourceDictionary { ["Nested"] = "n" };
		var source = new ResourceDictionary { ["LocalizedMessage"] = "en-US Message" };
		source.MergedDictionaries.Add(inner);
		GtkFormsXamlRegistry.Register("Round46/StringResources.xaml", () => source);

		var target = new ResourceDictionary { ["Old"] = "gone" };
		target.Source = Rel("Round46/StringResources.xaml");

		Assert.Equal("en-US Message", target["LocalizedMessage"]);
		Assert.Equal("n", target["Nested"]);
		Assert.False(target.ContainsKey("Old"));
		Assert.Equal(Rel("Round46/StringResources.xaml"), target.Source);
	}

	[Fact]
	public void Source_resolves_a_dictionary_added_to_MergedDictionaries()
	{
		GtkFormsXamlRegistry.Register("Round46/Merged.xaml", () => new ResourceDictionary { ["K"] = 1 });
		var app = new ResourceDictionary();

		app.MergedDictionaries.Add(new ResourceDictionary { Source = Rel("Round46/Merged.xaml") });

		Assert.Equal(1, app["K"]);
	}

	[Fact]
	public void Source_raises_ResourceChanged_for_the_loaded_keys()
	{
		GtkFormsXamlRegistry.Register("Round46/Changed.xaml", () => new ResourceDictionary { ["C"] = 2 });
		var target = new ResourceDictionary();
		object? seen = null;
		target.ResourceChanged += (k, _) => seen ??= k;

		target.Source = Rel("Round46/Changed.xaml");

		Assert.Equal("C", seen);
	}

	[Fact]
	public void Source_null_throws_ArgumentException() =>
		Assert.Throws<ArgumentException>(() => new ResourceDictionary().Source = null!);

	[Fact]
	public void Source_empty_throws_ArgumentException() =>
		Assert.Throws<ArgumentException>(() => new ResourceDictionary().Source = Rel(""));

	[Fact]
	public void Source_unknown_throws_IOException_and_keeps_the_dictionary()
	{
		var target = new ResourceDictionary { ["Keep"] = 1 };

		Assert.Throws<IOException>(() => target.Source = Rel("Round46/Missing.xaml"));

		Assert.Equal(1, target["Keep"]);
	}

	[Fact]
	public void Source_that_is_not_a_dictionary_throws_InvalidOperationException()
	{
		GtkFormsXamlRegistry.Register("Round46/NotADictionary.xaml", () => new object());

		Assert.Throws<InvalidOperationException>(
			() => new ResourceDictionary().Source = Rel("Round46/NotADictionary.xaml"));
	}
}
