using System.ComponentModel;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) for the Phase-0 one-way binding engine.
/// </summary>
public class BindingEngineTests
{
	// ── Helpers ─────────────────────────────────────────────────────────────

	sealed class ViewModel : INotifyPropertyChanged
	{
		string _greeting = "Hello";

		public string Greeting
		{
			get => _greeting;
			set
			{
				if (_greeting == value) return;
				_greeting = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Greeting)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	// ── Tests ────────────────────────────────────────────────────────────────

	[Fact]
	public void Binding_pushes_initial_value_from_DataContext()
	{
		var tb = new WpfTextBlock();
		var vm = new ViewModel { Greeting = "Hi" };

		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Greeting"));

		Assert.Equal("Hi", tb.Text);
	}

	[Fact]
	public void Binding_updates_on_INPC_change()
	{
		var tb = new WpfTextBlock();
		var vm = new ViewModel { Greeting = "Before" };

		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Greeting"));

		vm.Greeting = "After";

		Assert.Equal("After", tb.Text);
	}

	[Fact]
	public void Binding_with_explicit_Source()
	{
		var tb = new WpfTextBlock();
		var vm = new ViewModel { Greeting = "Direct" };

		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Greeting") { Source = vm });

		Assert.Equal("Direct", tb.Text);
	}

	[Fact]
	public void Binding_with_explicit_Source_updates_on_INPC()
	{
		var tb = new WpfTextBlock();
		var vm = new ViewModel { Greeting = "A" };

		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Greeting") { Source = vm });

		vm.Greeting = "B";
		Assert.Equal("B", tb.Text);
	}

	[Fact]
	public void Binding_refreshes_on_DataContext_change()
	{
		var tb = new WpfTextBlock();
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Greeting"));

		var vm1 = new ViewModel { Greeting = "First" };
		tb.DataContext = vm1;

		var vm2 = new ViewModel { Greeting = "Second" };
		tb.DataContext = vm2;

		Assert.Equal("Second", tb.Text);
	}

	[Fact]
	public void Binding_null_path_binds_to_source_itself()
	{
		var tb = new WpfTextBlock();
		tb.DataContext = "direct-string";
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding());

		Assert.Equal("direct-string", tb.Text);
	}

	[Fact]
	public void Binding_with_converter()
	{
		var tb = new WpfTextBlock();
		var vm = new ViewModel { Greeting = "hello" };
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Greeting") { Converter = new UpperCaseConverter() });

		Assert.Equal("HELLO", tb.Text);
	}

	sealed class UpperCaseConverter : IValueConverter
	{
		public object? Convert(object? value, System.Type targetType, object? parameter,
			System.Globalization.CultureInfo culture)
			=> (value as string)?.ToUpperInvariant();

		public object? ConvertBack(object? value, System.Type targetType, object? parameter,
			System.Globalization.CultureInfo culture)
			=> throw new System.NotSupportedException("TwoWay is Phase-1");
	}
}