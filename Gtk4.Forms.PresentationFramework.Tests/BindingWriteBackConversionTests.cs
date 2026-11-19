using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Write-back converts the target value to the source type and reports failures as validation errors.</summary>
public class BindingWriteBackConversionTests : WpfGtkTestBase
{
	sealed class Vm
	{
		public int Age { get; set; } = 1;
		public int? Maybe { get; set; } = 5;
		public string Name { get; set; } = "n";
	}

	sealed class ThrowingBackConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}

	static void MoveFocusAway(WpfTextBox tb)
	{
		tb.Focus();
		new WpfTextBox().Focus();
	}

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

	sealed class RecordingRule : ValidationRule
	{
		public object? Seen;
		public int Calls;
		public bool Required;

		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		{
			Calls++;
			Seen = value;
			return Required && string.IsNullOrEmpty(value as string)
				? new ValidationResult(false, "required")
				: ValidationResult.ValidResult;
		}
	}

	sealed class ParseConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> int.Parse((string)value!);
	}

	static WpfTextBox BindWithRule(Vm vm, RecordingRule rule, IValueConverter? converter = null)
	{
		var tb = new WpfTextBox { DataContext = vm };
		var b = new Binding("Age") { Converter = converter };
		b.ValidationRules.Add(rule);
		tb.SetBinding(WpfTextBox.TextProperty, b);
		return tb;
	}

	[Fact]
	public void Required_string_rule_on_int_source_sees_the_raw_text_and_allows_the_write() => Run(() =>
	{
		var vm = new Vm();
		var rule = new RecordingRule { Required = true };
		var tb = BindWithRule(vm, rule);

		tb.Text = "42";
		MoveFocusAway(tb);

		Assert.Equal(42, vm.Age);
		Assert.Equal("42", Assert.IsType<string>(rule.Seen));
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void Rule_runs_before_conversion_and_its_error_wins() => Run(() =>
	{
		var vm = new Vm { Age = 9 };
		var rule = new RecordingRule { Required = true };
		var tb = BindWithRule(vm, rule);

		tb.Text = "";
		MoveFocusAway(tb);

		Assert.Equal(9, vm.Age);
		Assert.Equal(1, rule.Calls);
		var error = Validation.GetErrors(tb)[0];
		Assert.Same(rule, error.RuleInError);
		Assert.Equal("required", error.ErrorContent);
	});

	[Fact]
	public void Passing_rule_runs_even_when_conversion_then_fails() => Run(() =>
	{
		var vm = new Vm { Age = 9 };
		var rule = new RecordingRule();
		var tb = BindWithRule(vm, rule);

		tb.Text = "abc";
		MoveFocusAway(tb);

		Assert.Equal(1, rule.Calls);
		Assert.Equal("abc", rule.Seen);
		Assert.Equal(9, vm.Age);
		Assert.True(Validation.GetHasError(tb));
	});

	[Fact]
	public void Rule_sees_raw_target_value_not_ConvertBack_output() => Run(() =>
	{
		var vm = new Vm();
		var rule = new RecordingRule();
		var tb = BindWithRule(vm, rule, new ParseConverter());

		tb.Text = "5";
		MoveFocusAway(tb);

		Assert.Equal("5", Assert.IsType<string>(rule.Seen));
		Assert.Equal(5, vm.Age);
	});
}
