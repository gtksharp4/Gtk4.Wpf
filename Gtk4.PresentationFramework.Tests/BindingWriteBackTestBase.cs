using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Shared view-model, converters, rule and helpers for the write-back conversion tests.</summary>
public abstract class BindingWriteBackTestBase : WpfGtkTestBase
{
	protected sealed class Vm
	{
		public int Age { get; set; } = 1;
		public int? Maybe { get; set; } = 5;
		public string Name { get; set; } = "n";
	}

	protected sealed class ThrowingBackConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}

	protected static void MoveFocusAway(WpfTextBox tb)
	{
		tb.Focus();
		new WpfTextBox().Focus();
	}

	protected sealed class RecordingRule : ValidationRule
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

	protected sealed class ParseConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> int.Parse((string)value!);
	}

	protected static WpfTextBox BindWithRule(Vm vm, RecordingRule rule, IValueConverter? converter = null)
	{
		var tb = new WpfTextBox { DataContext = vm };
		var b = new Binding("Age") { Converter = converter };
		b.ValidationRules.Add(rule);
		tb.SetBinding(WpfTextBox.TextProperty, b);
		return tb;
	}
}
