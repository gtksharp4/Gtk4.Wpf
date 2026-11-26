using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Owner-qualified paths whose owner carries an xmlns prefix, e.g. <c>(local:Data.Label)</c>.</summary>
public class PrefixedOwnerPathTests : WpfGtkTestBase
{
	sealed class Data : INotifyPropertyChanged
	{
		string? _label;
		public string? Label
		{
			get => _label;
			set { _label = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	static class Tagging
	{
		public static readonly DependencyProperty NoteProperty = DependencyProperty.RegisterAttached(
			"Note", typeof(string), typeof(Tagging), new PropertyMetadata(null));
		public static string? GetNote(DependencyObject d) => (string?)d.GetValue(NoteProperty);
		public static void SetNote(DependencyObject d, string? v) => d.SetValue(NoteProperty, v);
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("PrefixOut", typeof(string), typeof(TEl), new PropertyMetadata(null));
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	[Fact]
	public void Binding_strips_a_prefixed_source_type_owner() => Run(() =>
	{
		var data = new Data { Label = "one" };
		var el = new TEl { DataContext = data };
		el.SetBinding(TEl.OutProperty, new Binding("(local:Data.Label)"));
		Assert.Equal("one", el.Out);
		data.Label = "two";
		Assert.Equal("two", el.Out);
	});

	[Fact]
	public void Binding_with_a_prefixed_foreign_owner_stays_an_attached_property_path() => Run(() =>
	{
		var holder = new Border();
		Tagging.SetNote(holder, "n1");
		var el = new TEl { DataContext = holder };
		el.SetBinding(TEl.OutProperty, new Binding("(local:Tagging.Note)"));
		Assert.Equal("n1", el.Out);
		Tagging.SetNote(holder, "n2");
		Assert.Equal("n2", el.Out);
	});

	[Fact]
	public void DataTrigger_with_a_prefixed_owner_matches_the_source_value() => Run(() =>
	{
		var data = new Data { Label = "off" };
		var el = new TEl { DataContext = data };
		var style = new Style(typeof(TEl));
		var trigger = new DataTrigger { Binding = new Binding("(local:Data.Label)"), Value = "on" };
		trigger.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "hit" });
		style.Triggers.Add(trigger);
		el.Style = style;
		Assert.Null(el.Out);
		data.Label = "on";
		Assert.Equal("hit", el.Out);
		data.Label = "off";
		Assert.Null(el.Out);
	});

	[Fact]
	public void Owner_that_is_not_the_source_type_is_not_stripped_to_a_same_named_member() => Run(() =>
	{
		var el = new TEl { DataContext = new Data { Label = "clr" } };
		el.SetBinding(TEl.OutProperty, new Binding("(local:Other.Label)") { FallbackValue = "fb" });
		Assert.Equal("fb", el.Out);
	});
}
