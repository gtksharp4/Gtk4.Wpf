using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>SpellCheck attached properties are stored only.</summary>
public class SpellCheckTests : WpfGtkTestBase
{
	[Fact]
	public void IsEnabled_defaults_to_false_and_round_trips() => Run(() =>
	{
		var tb = new TextBox();
		Assert.False(SpellCheck.GetIsEnabled(tb));
		SpellCheck.SetIsEnabled(tb, true);
		Assert.True(SpellCheck.GetIsEnabled(tb));
	});

	[Fact]
	public void CustomDictionaries_is_created_once_per_element() => Run(() =>
	{
		var tb = new TextBox();
		var first = SpellCheck.GetCustomDictionaries(tb);
		Assert.NotNull(first);
		Assert.Same(first, SpellCheck.GetCustomDictionaries(tb));
		Assert.NotSame(first, SpellCheck.GetCustomDictionaries(new TextBox()));
	});

	[Fact]
	public void CustomDictionaries_is_the_non_generic_IList_and_takes_Uris() => Run(() =>
	{
		System.Collections.IList list = SpellCheck.GetCustomDictionaries(new TextBox());
		list.Add(new Uri("file:///d.lex"));
		Assert.Single(list);
		Assert.Equal(typeof(System.Collections.IList), SpellCheck.CustomDictionariesProperty.PropertyType);
	});

	[Fact]
	public void Accessors_reject_a_null_element()
	{
		Assert.Throws<ArgumentNullException>(() => SpellCheck.GetIsEnabled(null!));
		Assert.Throws<ArgumentNullException>(() => SpellCheck.SetIsEnabled(null!, true));
		Assert.Throws<ArgumentNullException>(() => SpellCheck.GetCustomDictionaries(null!));
	}
}
