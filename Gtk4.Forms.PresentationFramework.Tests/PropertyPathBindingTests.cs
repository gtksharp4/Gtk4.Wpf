using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class PropertyPathBindingTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	class Item
	{
		public string Age { get; set; } = "42";
	}

	[Fact]
	public void Path_is_null_by_default_and_set_by_the_string_constructor() => Run(() =>
	{
		Assert.Null(new Binding().Path);
		Assert.Null(new Binding(null!).Path);
		Assert.Equal("A.B", new Binding("A.B").Path!.Path);
		Assert.Empty(new Binding("A").Path!.PathParameters);
	});

	[Fact]
	public void Generated_string_assignment_compiles_through_the_bridge() => Run(() =>
	{
		var b = new Binding();
		b.Path = "Age";

		Assert.Equal("Age", b.Path!.Path);
	});

	[Fact]
	public void A_PropertyPath_binds_like_the_string() => Run(() =>
	{
		var text = new TextBlock { DataContext = new Item() };
		text.SetBinding(TextBlock.TextProperty, new Binding { Path = new PropertyPath("Age") });

		Assert.Equal("42", text.Text);
	});

	[Fact]
	public void A_dependency_property_path_resolves_against_a_dependency_object_source() => Run(() =>
	{
		var source = new Border { Width = 77 };
		var target = new TextBlock();
		target.SetBinding(TextBlock.TextProperty,
			new Binding { Source = source, Path = new PropertyPath(FrameworkElement.WidthProperty) });
		Assert.Equal("77", target.Text);

		var viaPlaceholder = new TextBlock();
		viaPlaceholder.SetBinding(TextBlock.TextProperty,
			new Binding { Source = source, Path = new PropertyPath("(0)", FrameworkElement.WidthProperty) });
		Assert.Equal("77", viaPlaceholder.Text);
	});

	[Fact]
	public void An_unresolvable_placeholder_path_is_a_binding_error_not_a_crash() => Run(() =>
	{
		var target = new TextBlock { Text = "x" };
		target.SetBinding(TextBlock.TextProperty,
			new Binding { Source = new Item(), Path = new PropertyPath("(0).(1)", "a", "b"), FallbackValue = "fb" });

		Assert.Equal("fb", target.Text);
	});

	[Fact]
	public void Converter_round_trips_a_path_string()
	{
		var converter = new PropertyPathConverter();

		var path = (PropertyPath)converter.ConvertFromInvariantString("Address.City")!;

		Assert.Equal("Address.City", path.Path);
		Assert.Equal("Address.City", converter.ConvertToInvariantString(path));
		Assert.True(converter.CanConvertFrom(typeof(string)));
		Assert.False(converter.CanConvertFrom(typeof(int)));
		Assert.Equal(typeof(PropertyPathConverter),
			System.ComponentModel.TypeDescriptor.GetConverter(typeof(PropertyPath)).GetType());
	}

	class Throwing : System.ComponentModel.INotifyPropertyChanged
	{
		public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
		public double Boom => throw new InvalidOperationException("not begun");
	}

	[Fact]
	public void A_throwing_source_getter_is_a_binding_error_not_a_crash() => Run(() =>
	{
		var target = new TextBlock { Text = "x" };
		target.SetBinding(TextBlock.TextProperty,
			new Binding { Source = new Throwing(), Path = new PropertyPath("Boom"), FallbackValue = "fb" });

		Assert.Equal("fb", target.Text);
	});

	const string Ns = "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'";

	[Fact]
	public void XamlReader_parses_the_attribute_binding_form() => Run(() =>
	{
		var text = (TextBlock)XamlReader.Parse($"<TextBlock {Ns} Text='{{Binding X}}'/>");

		Assert.Equal("X", BindingOperations.GetBinding(text, TextBlock.TextProperty)!.Path!.Path);
	});

	[Fact]
	public void XamlReader_parses_the_explicit_Path_form() => Run(() =>
	{
		var text = (TextBlock)XamlReader.Parse($"<TextBlock {Ns} Text='{{Binding Path=X}}'/>");

		Assert.Equal("X", BindingOperations.GetBinding(text, TextBlock.TextProperty)!.Path!.Path);
	});

	[Fact]
	public void XamlReader_parses_the_binding_element_form() => Run(() =>
	{
		var text = (TextBlock)XamlReader.Parse(
			$"<TextBlock {Ns}><TextBlock.Text><Binding Path='X'/></TextBlock.Text></TextBlock>");

		Assert.Equal("X", BindingOperations.GetBinding(text, TextBlock.TextProperty)!.Path!.Path);
	});

	[Fact]
	public void Assigning_a_null_string_leaves_the_path_unset_and_binds_the_source_itself() => Run(() =>
	{
		string? s = null;
		var b = new Binding();
		b.Path = s;
		Assert.Null(b.Path);

		b.Source = "abc";
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, b);

		Assert.Equal("abc", text.Text);
	});

	[Fact]
	public void Converter_rejects_a_null_source()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new PropertyPathConverter().ConvertFrom(null!, System.Globalization.CultureInfo.InvariantCulture, null!));
	}

	[Fact]
	public void Converter_writes_a_dependency_property_path_owner_qualified_and_round_trips() => Run(() =>
	{
		var conv = new PropertyPathConverter();
		var text = (string)conv.ConvertTo(new PropertyPath(TextBlock.TextProperty), typeof(string))!;
		Assert.Equal("(TextBlock.Text)", text);

		var back = (PropertyPath)conv.ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, text)!;
		var target = new TextBlock();
		target.SetBinding(TextBlock.TextProperty,
			new Binding { Path = back, Source = new TextBlock { Text = "round" } });
		Assert.Equal("round", target.Text);
	});

	class Positioned
	{
		public double Left { get; set; } = 7;
	}

	[Fact]
	public void Attached_owner_path_does_not_read_a_same_named_clr_member() => Run(() =>
	{
		var target = new TextBlock();
		target.SetBinding(TextBlock.TextProperty,
			new Binding { Path = new PropertyPath("(Canvas.Left)"), Source = new Positioned(), FallbackValue = "none" });
		Assert.Equal("none", target.Text);
	});
}
