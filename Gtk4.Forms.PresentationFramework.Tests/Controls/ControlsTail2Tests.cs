using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Gtk4.Forms.PresentationFramework.Tests.XamlBacked;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class ControlsTail2Tests : WpfGtkTestBase
{
	[Fact]
	public void Hyperlink_TargetName_metadata_and_event_target()
	{
		Assert.Equal(string.Empty, Hyperlink.TargetNameProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(typeof(string), Hyperlink.TargetNameProperty.PropertyType);
		var link = new Hyperlink { NavigateUri = new Uri("http://x/a"), TargetName = "frame2" };
		Assert.Equal("frame2", link.TargetName);
		RequestNavigateEventArgs? got = null;
		link.RequestNavigate += (s, e) => got = e;
		link.OnClick();
		Assert.Equal("frame2", got!.Target);
		Assert.Null(new RequestNavigateEventArgs(new Uri("http://x"), null).Target);
		Assert.Equal(string.Empty, new Hyperlink().TargetName);
	}

	[Fact]
	public void FrameworkElement_Tag_round_trips_and_is_shared_with_content_elements() => Run(() =>
	{
		var item = new ComboBoxItem { Tag = "t" };
		Assert.Equal("t", item.Tag);
		Assert.Equal("t", item.GetValue(FrameworkElement.TagProperty));
		Assert.Null(FrameworkElement.TagProperty.DefaultMetadata.DefaultValue);
		Assert.True(FrameworkContentElement.TagProperty.OwnerType == typeof(FrameworkElement)
			|| FrameworkContentElement.TagProperty == FrameworkElement.TagProperty);
	});

	[Theory]
	[InlineData("Normal")]
	[InlineData("italic")]
	[InlineData("OBLIQUE")]
	public void FontStyleConverter_parses_names(string text)
	{
		var c = new FontStyleConverter();
		Assert.True(c.CanConvertFrom(typeof(string)));
		Assert.True(c.CanConvertTo(typeof(string)));
		var style = (FontStyle)c.ConvertFrom(text)!;
		Assert.Equal(text, (string)c.ConvertTo(style, typeof(string))!, ignoreCase: true);
		Assert.IsType<FontStyleConverter>(TypeDescriptor.GetConverter(typeof(FontStyle)));
	}

	[Theory]
	[InlineData("Thin", 100)]
	[InlineData("ultralight", 200)]
	[InlineData("Regular", 400)]
	[InlineData("SemiBold", 600)]
	[InlineData("UltraBold", 800)]
	[InlineData("Heavy", 900)]
	[InlineData("ExtraBlack", 950)]
	[InlineData("123", 123)]
	public void FontWeightConverter_parses_names_and_numbers(string text, int weight)
	{
		var c = new FontWeightConverter();
		Assert.Equal(weight, ((FontWeight)c.ConvertFrom(text)!).ToOpenTypeWeight());
		Assert.IsType<FontWeightConverter>(TypeDescriptor.GetConverter(typeof(FontWeight)));
	}

	[Fact]
	public void Font_converters_throw_the_WPF_exception_types()
	{
		var w = new FontWeightConverter();
		Assert.Throws<FormatException>(() => w.ConvertFrom("heavier"));
		Assert.Throws<ArgumentOutOfRangeException>(() => w.ConvertFrom("1000"));
		Assert.Throws<ArgumentOutOfRangeException>(() => w.ConvertFrom("0"));
		Assert.Throws<NotSupportedException>(() => w.ConvertFrom(null!));
		Assert.Equal("value", Assert.Throws<ArgumentException>(() => w.ConvertFrom(5)).ParamName);
		Assert.Equal("value", Assert.Throws<ArgumentException>(() => new FontStyleConverter().ConvertFrom(5)).ParamName);
		Assert.Throws<FormatException>(() => new FontStyleConverter().ConvertFrom("slanted"));
		Assert.Throws<NotSupportedException>(() => new FontStyleConverter().ConvertFrom(null!));
		Assert.Equal("Bold", w.ConvertTo(FontWeights.Bold, typeof(string)));
		Assert.False(w.CanConvertTo(typeof(int)));
		Assert.Equal("5", w.ConvertTo(5, typeof(string)));
		Assert.Throws<NotSupportedException>(() => w.ConvertTo(FontWeights.Bold, typeof(int)));
		Assert.Throws<NotSupportedException>(() => new FontStyleConverter().ConvertTo(FontStyles.Italic, typeof(int)));
	}

	[Fact]
	public void Font_converters_round_trip_through_InstanceDescriptor()
	{
		var w = new FontWeightConverter();
		Assert.True(w.CanConvertTo(typeof(InstanceDescriptor)));
		var wd = (InstanceDescriptor)w.ConvertTo(FontWeights.Bold, typeof(InstanceDescriptor))!;
		Assert.Equal(FontWeights.Bold, (FontWeight)wd.Invoke()!);
		Assert.Equal(typeof(FontWeight).GetMethod("FromOpenTypeWeight", new[] { typeof(int) }), wd.MemberInfo);

		var s = new FontStyleConverter();
		Assert.True(s.CanConvertTo(typeof(InstanceDescriptor)));
		var sd = (InstanceDescriptor)s.ConvertTo(FontStyles.Oblique, typeof(InstanceDescriptor))!;
		Assert.Equal(FontStyles.Oblique, (FontStyle)sd.Invoke()!);
		Assert.Equal(typeof(FontStyles).GetProperty("Oblique"), sd.MemberInfo);
	}
	[Fact]
	public void InkCanvas_defaults_and_enum_values()
	{
		Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 6 }, Array.ConvertAll(
			(InkCanvasEditingMode[])Enum.GetValues(typeof(InkCanvasEditingMode)), v => (int)v));
		Assert.Equal(InkCanvasEditingMode.Ink, InkCanvas.EditingModeProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(InkCanvasEditingMode.EraseByStroke,
			InkCanvas.EditingModeInvertedProperty.DefaultMetadata.DefaultValue);
		Assert.True(InkCanvas.ActiveEditingModeProperty.ReadOnly);
		Assert.True(double.IsNaN(Convert.ToDouble(InkCanvas.LeftProperty.DefaultMetadata.DefaultValue)));
	}

	[Fact]
	public void InkCanvas_raises_change_events_and_updates_ActiveEditingMode() => Run(() =>
	{
		var ink = new InkCanvas();
		int modes = 0, inverted = 0;
		ink.EditingModeChanged += (s, e) => modes++;
		ink.EditingModeInvertedChanged += (s, e) => inverted++;
		ink.EditingMode = InkCanvasEditingMode.Select;
		ink.EditingModeInverted = InkCanvasEditingMode.EraseByPoint;
		Assert.Equal((1, 1), (modes, inverted));
		Assert.Equal(InkCanvasEditingMode.Select, ink.ActiveEditingMode);
		Assert.Throws<ArgumentException>(() => ink.EditingMode = (InkCanvasEditingMode)99);
	});

	[Fact]
	public void InkCanvas_hosts_children_and_the_markup_compiles() => Run(() =>
	{
		var w = new InkWindow();
		Assert.Equal(InkCanvasEditingMode.Select, w.Ink.EditingMode);
		Assert.Single(w.Ink.Children);
		Assert.Equal(20.0, InkCanvas.GetLeft(w.Pinned));
		Assert.NotNull(w.Ink.Widget);
	});

	[Fact]
	public void TextBox_TextAlignment_metadata_and_widget_mapping() => Run(() =>
	{
		var meta = (FrameworkPropertyMetadata)TextBox.TextAlignmentProperty.GetMetadata(typeof(TextBox));
		Assert.Equal(TextAlignment.Left, meta.DefaultValue);
		Assert.True(meta.Inherits);
		var single = new TextBox { TextAlignment = TextAlignment.Right };
		Assert.Equal(1f, ((Gtk.Entry)single.Widget).Alignment);
		single.TextAlignment = TextAlignment.Center;
		Assert.Equal(0.5f, ((Gtk.Entry)single.Widget).Alignment);
		var multi = new TextBox { AcceptsReturn = true, TextAlignment = TextAlignment.Justify };
		_ = multi.Widget;
		var view = (Gtk.TextView)multi.FocusWidget;
		Assert.Equal(Gtk.Justification.Fill, view.Justification);
		multi.TextAlignment = TextAlignment.Center;
		Assert.Equal(Gtk.Justification.Center, view.Justification);
	});

	[Fact]
	public void TextBox_line_limits_have_WPF_defaults_and_validation()
	{
		var tb = new TextBox();
		Assert.Equal(int.MaxValue, tb.MaxLines);
		Assert.Equal(1, tb.MinLines);
		tb.MaxLines = 5;
		tb.MinLines = 2;
		Assert.Equal((5, 2), (tb.MaxLines, tb.MinLines));
		Assert.Throws<ArgumentException>(() => tb.MaxLines = 0);
		Assert.Throws<ArgumentException>(() => tb.MinLines = -1);
	}

	[Fact]
	public void InkCanvas_children_join_the_logical_tree() => Run(() =>
	{
		var button = new Button { Width = 40, Height = 20 };
		var ink = new InkCanvas { Width = 100, Height = 100 };
		ink.Children.Add(button);
		var w = new Window { DataContext = "ctx", Content = ink, Width = 300, Height = 200 };
		w.Resources["Key"] = "found";
		w.Show();
		Assert.True(GtkPump.Until(() => button.IsLoaded));
		Assert.Equal("ctx", button.DataContext);
		Assert.True(button.IsVisible);
		Assert.Same(ink, button.Parent);
		Assert.Equal("found", button.FindResource("Key"));
	});
}
