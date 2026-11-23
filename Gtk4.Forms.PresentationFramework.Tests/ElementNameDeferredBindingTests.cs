using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using DRun = System.Windows.Documents.Run;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>ElementName bindings set before the target is in the tree, in generated InitializeComponent order.</summary>
public class ElementNameDeferredBindingTests : WpfGtkTestBase
{
	static Binding ToSlider(string path = "Value") => new Binding(path) { ElementName = "slider" };

	[Fact]
	public void Binding_set_before_the_tree_and_the_name_resolves_and_keeps_following() => Run(() =>
	{
		var root = new StackPanel();
		var slider = new Slider { Maximum = 100, Value = 5 };
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, ToSlider());
		Assert.Equal("", text.Text);

		root.Children.Add(slider);
		root.Children.Add(text);
		root.RegisterName("slider", slider);

		Assert.Equal("5", text.Text);
		slider.Value = 7;
		Assert.Equal("7", text.Text);
		slider.Value = 9;
		Assert.Equal("9", text.Text);
	});

	[Fact]
	public void Name_registered_before_the_target_joins_the_tree_resolves_on_attach() => Run(() =>
	{
		var root = new StackPanel();
		var slider = new Slider { Maximum = 100, Value = 3 };
		root.Children.Add(slider);
		root.RegisterName("slider", slider);
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, ToSlider());

		root.Children.Add(text);

		Assert.Equal("3", text.Text);
		slider.Value = 4;
		Assert.Equal("4", text.Text);
	});

	[Fact]
	public void Pending_binding_shows_the_FallbackValue() => Run(() =>
	{
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty,
			new Binding("Value") { ElementName = "slider", FallbackValue = "none" });
		Assert.Equal("none", text.Text);
	});

	[Fact]
	public void TwoWay_writes_back_after_late_resolution() => Run(() =>
	{
		var root = new StackPanel();
		var slider = new Slider { Maximum = 100, Value = 5 };
		var other = new Slider { Maximum = 100 };
		other.SetBinding(Slider.ValueProperty,
			new Binding("Value") { ElementName = "slider", Mode = BindingMode.TwoWay });
		root.Children.Add(slider);
		root.Children.Add(other);
		root.RegisterName("slider", slider);

		Assert.Equal(5, other.Value);
		other.Value = 20;
		Assert.Equal(20, slider.Value);
	});

	[Fact]
	public void Source_replaced_in_the_scope_re_resolves() => Run(() =>
	{
		var root = new StackPanel();
		var first = new Slider { Maximum = 100, Value = 1 };
		var second = new Slider { Maximum = 100, Value = 2 };
		var text = new TextBlock();
		root.Children.Add(text);
		root.RegisterName("slider", first);
		text.SetBinding(TextBlock.TextProperty, ToSlider());
		Assert.Equal("1", text.Text);

		root.UnregisterName("slider");
		root.RegisterName("slider", second);

		Assert.Equal("2", text.Text);
		first.Value = 50;
		Assert.Equal("2", text.Text);
		second.Value = 8;
		Assert.Equal("8", text.Text);
	});

	[Fact]
	public void Content_element_target_resolves_a_late_name() => Run(() =>
	{
		var root = new StackPanel();
		var box = new TextBox { Text = "typed" };
		var block = new TextBlock();
		var run = new DRun();
		run.SetBinding(DRun.TextProperty, new Binding("Text") { ElementName = "box" });
		block.Inlines.Add(run);
		root.Children.Add(box);
		root.Children.Add(block);
		root.RegisterName("box", box);

		Assert.Equal("typed", run.Text);
		box.Text = "again";
		Assert.Equal("again", run.Text);
	});

	[Fact]
	public void Name_registered_after_the_window_is_loaded_resolves() => Run(() =>
	{
		var slider = new Slider { Maximum = 100, Value = 6 };
		var text = new TextBlock();
		var root = new StackPanel();
		root.Children.Add(slider);
		root.Children.Add(text);
		var window = new Window { Content = root };
		text.SetBinding(TextBlock.TextProperty, ToSlider());
		window.Show();
		try
		{
			root.RegisterName("slider", slider);
			Assert.Equal("6", text.Text);
			slider.Value = 12;
			Assert.Equal("12", text.Text);
		}
		finally { window.Close(); }
	});
}
