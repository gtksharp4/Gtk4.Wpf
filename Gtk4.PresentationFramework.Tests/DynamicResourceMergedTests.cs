using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pins live <c>{DynamicResource}</c> tracking through MERGED dictionaries
/// (Round-17 M2): a key resolved from a merged dictionary must also live-update
/// when that merged dictionary is mutated.
/// </summary>
public class DynamicResourceMergedTests
{
	sealed class El : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register(
				"MergedDynResTag", typeof(string), typeof(El), new PropertyMetadata(null));

		public new string? Tag
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void SetResourceReference_resolves_from_merged_dictionary()
	{
		var el = new El();
		var merged = new ResourceDictionary { ["color"] = "Red" };
		el.Resources.MergedDictionaries.Add(merged);

		el.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Red", el.Tag);
	}

	[Fact]
	public void SetResourceReference_updates_when_merged_dictionary_entry_changes()
	{
		var el = new El();
		var merged = new ResourceDictionary { ["color"] = "Red" };
		el.Resources.MergedDictionaries.Add(merged);
		el.SetResourceReference(El.TagProperty, "color");
		Assert.Equal("Red", el.Tag);

		// Mutating the merged dictionary must push the new value live.
		merged["color"] = "Blue";

		Assert.Equal("Blue", el.Tag);
	}
}
