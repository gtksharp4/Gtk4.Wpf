using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Write-back converts the target value to the source type and reports failures as validation errors.</summary>
public class BindingWriteBackConversionTests : BindingWriteBackTestBase
{
	[Fact]
	public void TextBox_text_converts_to_an_int_source_on_LostFocus() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Age"));

		tb.Text = "42";
		MoveFocusAway(tb);

		Assert.Equal(42, vm.Age);
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void Invalid_text_leaves_the_source_and_reports_a_validation_error_until_a_valid_write() => Run(() =>
	{
		var vm = new Vm { Age = 9 };
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Age"));

		tb.Text = "abc";
		MoveFocusAway(tb);
		Assert.Equal(9, vm.Age);
		Assert.True(Validation.GetHasError(tb));

		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(7, vm.Age);
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void Empty_text_writes_null_to_a_nullable_source() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Maybe"));

		tb.Text = "";
		MoveFocusAway(tb);

		Assert.Null(vm.Maybe);
	});

	[Fact]
	public void Slider_double_value_writes_to_an_int_source_and_recoercion_does_not_throw() => Run(() =>
	{
		var vm = new Vm();
		var slider = new Slider { DataContext = vm, Maximum = 10 };
		slider.SetBinding(Slider.ValueProperty, new Binding("Age"));

		slider.Value = 3.0;
		Assert.Equal(3, vm.Age);

		slider.Value = 8;
		slider.Maximum = 5;

		Assert.Equal(5, vm.Age);
	});

	[Fact]
	public void ConvertBack_that_throws_is_swallowed_and_the_source_is_unchanged() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name") { Converter = new ThrowingBackConverter() });

		tb.Text = "changed";
		MoveFocusAway(tb);

		Assert.Equal("n", vm.Name);
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void ConvertBack_that_throws_reports_an_error_when_ValidatesOnExceptions() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty,
			new Binding("Name") { Converter = new ThrowingBackConverter(), ValidatesOnExceptions = true });

		tb.Text = "changed";
		MoveFocusAway(tb);

		Assert.Equal("n", vm.Name);
		Assert.True(Validation.GetHasError(tb));
	});
}
