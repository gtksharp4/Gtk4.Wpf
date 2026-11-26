using System;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// DataTrigger path attachment contract: multi-segment and single-segment paths
/// both attach without exception (E5 restriction lifted).
/// </summary>
public class DataTriggerPathContractTests
{
	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("DtPathTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Multi_segment_path_attaches_without_exception()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var dt = new DataTrigger
		{
			Binding = new Binding("Parent.Child"),
			Value = "x"
		};
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "hit" });
		style.Triggers.Add(dt);

		// Must not throw — nested paths are now supported.
		var ex = Record.Exception(() => el.Style = style);
		Assert.Null(ex);
	}

	[Fact]
	public void Single_segment_path_attaches_without_exception()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var dt = new DataTrigger
		{
			Binding = new Binding("Active"),
			Value = "yes"
		};
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "ok" });
		style.Triggers.Add(dt);

		var ex = Record.Exception(() => el.Style = style);
		Assert.Null(ex);
	}
}
