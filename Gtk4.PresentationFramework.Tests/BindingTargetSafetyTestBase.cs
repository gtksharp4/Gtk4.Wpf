using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Shared view-models, converters and binding factories for the binding target-safety tests.</summary>
public abstract class BindingTargetSafetyTestBase : WpfGtkTestBase
{
	protected sealed class NullableVm : INotifyPropertyChanged
	{
		double? _size;
		string? _text;

		public double? Size
		{
			get => _size;
			set { _size = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size))); }
		}

		public string? Text
		{
			get => _text;
			set { _text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	protected sealed class JoinConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) => null;
		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => null;
	}

	protected static MultiBinding NoSourceMultiBinding()
	{
		var mb = new MultiBinding { Converter = new JoinConverter() };
		mb.Bindings.Add(new Binding("Missing"));
		return mb;
	}

	protected sealed class FuncConverter : IValueConverter
	{
		public Func<object?, object?> Func = v => v;
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Func(value);
		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
	}

	protected sealed class FuncMultiConverter : IMultiValueConverter
	{
		public Func<object?[], object?> Func = v => v[0];
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> Func(values);
		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
			=> null;
	}

	protected sealed class IntVm
	{
		public int W { get; set; } = 40;
	}

	protected static FuncConverter Unset() => new FuncConverter { Func = _ => DependencyProperty.UnsetValue };

	protected static MultiBinding Multi(NullableVm vm, Func<object?[], object?> func, object? fallback = null)
	{
		var mb = new MultiBinding { Converter = new FuncMultiConverter { Func = func } };
		mb.Bindings.Add(new Binding("Size") { Source = vm });
		if (fallback != null) mb.FallbackValue = fallback;
		return mb;
	}
}
