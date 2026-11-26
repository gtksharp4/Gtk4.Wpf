using System;
using System.Windows;
using System.Windows.Markup;
using Gtk4.PresentationFramework.Tests;
using Xunit;

[assembly: GtkFormsXamlRegistration("Round47/D.xaml", typeof(Round47Dictionary))]

namespace Gtk4.PresentationFramework.Tests;

public sealed class Round47Dictionary : ResourceDictionary
{
	public Round47Dictionary() => this["FromD"] = "d";
}

/// <summary>Pack-style <c>/Asm;component/path</c> URIs resolve against the named assembly's registrations.</summary>
public class RegistryAssemblyLookupTests
{
	static readonly string AsmName = typeof(Round47Dictionary).Assembly.GetName().Name!;

	static Uri Rel(string path) => new Uri(path, UriKind.RelativeOrAbsolute);

	[Theory]
	[InlineData("/{0};component/Round47/D.xaml")]
	[InlineData("pack://application:,,,/{0};component/Round47/D.xaml")]
	[InlineData("/{0};v1.0.0.0;component/Round47/D.xaml")]
	public void A_URI_naming_the_assembly_resolves_its_registration(string format)
	{
		object? created = GtkFormsXamlRegistry.TryCreate(UnitConverters.ParseUri(string.Format(format, AsmName)));

		Assert.IsType<Round47Dictionary>(created);
	}

	[Fact]
	public void A_URI_naming_another_assembly_does_not_fall_back_to_a_same_named_file_elsewhere()
	{
		Assert.Null(GtkFormsXamlRegistry.TryCreate(Rel("/Missing;component/Round47/D.xaml")));
		Assert.Null(GtkFormsXamlRegistry.TryCreate(Rel("/Missing;component/D.xaml")));
	}

	[Fact]
	public void A_URI_without_an_assembly_finds_a_registration_in_any_loaded_assembly()
	{
		Assert.IsType<Round47Dictionary>(GtkFormsXamlRegistry.TryCreate(Rel("Round47/D.xaml"), assembly: null));
	}

	[Fact]
	public void An_unresolvable_merged_Source_goes_through_the_generated_path_without_throwing()
	{
		var owner = new ResourceDictionary { ["Keep"] = 1 };

		// Shape of the generated merge: load into a scratch dictionary, add only when it resolved.
		var missing = new ResourceDictionary();
		if (GtkFormsXamlRegistry.TryLoadDictionary(missing, Rel("/Missing;component/X.xaml")))
			owner.MergedDictionaries.Add(missing);
		var found = new ResourceDictionary();
		if (GtkFormsXamlRegistry.TryLoadDictionary(found, Rel($"/{AsmName};component/Round47/D.xaml")))
			owner.MergedDictionaries.Add(found);

		Assert.Equal(1, owner["Keep"]);
		Assert.Equal("d", owner["FromD"]);
		Assert.Single(owner.MergedDictionaries);
		Assert.False(missing.ContainsKey("FromD"));
	}

	[Fact]
	public void The_public_Source_setter_still_throws_for_an_unresolvable_URI()
	{
		Assert.Throws<System.IO.IOException>(() => new ResourceDictionary().Source = Rel("/Missing;component/X.xaml"));
	}
}
