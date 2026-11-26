using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="MultiBinding"/> TwoWay: ConvertBack writes each child source.
/// Each test fails without the feature and passes with it.
/// </summary>
public class MultiBindingTwoWayTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	sealed class FullName
	{
		public string First { get; set; } = string.Empty;
		public string Last { get; set; } = string.Empty;
	}

	sealed class MutableVm : INotifyPropertyChanged
	{
		string _first = string.Empty;
		string _last = string.Empty;

		public string First
		{
			get => _first;
			set { _first = value; Raise(nameof(First)); }
		}

		public string Last
		{
			get => _last;
			set { _last = value; Raise(nameof(Last)); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		void Raise(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
	}

	sealed class StringElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(StringElement),
				new PropertyMetadata(null));
		public string? Value
		{
			get => (string?)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// Splits "First Last" back into two strings; passes them back via ConvertBack.
	/// </summary>
	sealed class SplitConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> $"{values[0]} {values[1]}";

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
		{
			var s = value?.ToString() ?? string.Empty;
			var parts = s.Split(' ');
			return new object?[]
			{
				parts.Length > 0 ? parts[0] : string.Empty,
				parts.Length > 1 ? parts[1] : string.Empty
			};
		}
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

	[Fact]
	public void MultiBinding_TwoWay_ConvertBack_writes_each_source()
	{
		var vm = new MutableVm { First = "John", Last = "Doe" };
		var el = new StringElement { DataContext = vm };

		var mb = new MultiBinding
		{
			Converter = new SplitConverter(),
			Mode = BindingMode.TwoWay
		};
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		el.SetBinding(StringElement.ValueProperty, mb);

		// Initial forward push.
		Assert.Equal("John Doe", el.Value);

		// Mutate target → ConvertBack → both sources updated.
		el.Value = "Jane Smith";

		Assert.Equal("Jane", vm.First);
		Assert.Equal("Smith", vm.Last);
	}

	[Fact]
	public void MultiBinding_TwoWay_forward_push_still_works()
	{
		var vm = new MutableVm { First = "Alice", Last = "Wonder" };
		var el = new StringElement { DataContext = vm };

		var mb = new MultiBinding
		{
			Converter = new SplitConverter(),
			Mode = BindingMode.TwoWay
		};
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		el.SetBinding(StringElement.ValueProperty, mb);

		Assert.Equal("Alice Wonder", el.Value);

		vm.First = "Bob";
		Assert.Equal("Bob Wonder", el.Value);
	}

	[Fact]
	public void MultiBinding_TwoWay_DoNothing_skips_that_source()
	{
		var vm = new MutableVm { First = "X", Last = "Y" };
		var el = new StringElement { DataContext = vm };

		// Converter returns DoNothing for first value → only Last updated.
		var mb = new MultiBinding
		{
			Converter = new SkipFirstConverter(),
			Mode = BindingMode.TwoWay
		};
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		el.SetBinding(StringElement.ValueProperty, mb);

		el.Value = "X newLast";

		Assert.Equal("X", vm.First);       // skipped
		Assert.Equal("newLast", vm.Last);  // updated
	}

	sealed class SkipFirstConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> $"{values[0]} {values[1]}";

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
		{
			var parts = (value?.ToString() ?? string.Empty).Split(' ');
			return new object?[]
			{
				Binding.DoNothing,
				parts.Length > 1 ? parts[1] : string.Empty
			};
		}
	}
}
