using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) for Phase-1 binding features:
/// TwoWay, OneTime, OneWayToSource, ElementName, ConvertBack, no-feedback loop.
/// </summary>
public class BindingEnginePhase1Tests
{
	// ── Helpers ─────────────────────────────────────────────────────────────

	sealed class ViewModel : INotifyPropertyChanged
	{
		string _text = string.Empty;

		public string Text
		{
			get => _text;
			set
			{
				if (_text == value) return;
				_text = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	/// Converter that appends/removes a suffix so ConvertBack is distinguishable.
	sealed class SuffixConverter : IValueConverter
	{
		public object? Convert(object? value, System.Type t, object? param, CultureInfo c)
			=> value is string s ? s + "_fwd" : value;

		public object? ConvertBack(object? value, System.Type t, object? param, CultureInfo c)
		{
			if (value is string s && s.EndsWith("_fwd"))
				return s.Substring(0, s.Length - 4);
			return value;
		}
	}

	// Minimal FrameworkElement subclass that exposes a writable DP for testing.
	sealed class TestElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(TestElement),
				new PropertyMetadata(null));

		public string? Value
		{
			get => (string?)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => throw new System.NotSupportedException();
	}

	// ── TwoWay: source -> target ─────────────────────────────────────────────

	[Fact]
	public void TwoWay_source_change_updates_target()
	{
		var vm = new ViewModel { Text = "initial" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Text") { Mode = BindingMode.TwoWay });

		Assert.Equal("initial", tb.Text);

		vm.Text = "updated";
		Assert.Equal("updated", tb.Text);
	}

	// ── TwoWay: target -> source (write-back) ────────────────────────────────

	[Fact]
	public void TwoWay_target_change_writes_back_to_source()
	{
		var vm = new ViewModel { Text = "original" };
		var el = new TestElement();
		el.DataContext = vm;
		el.SetBinding(TestElement.ValueProperty,
			new Binding("Text") { Mode = BindingMode.TwoWay });

		Assert.Equal("original", el.Value);

		el.Value = "edited";

		Assert.Equal("edited", vm.Text);
	}

	// ── TwoWay no feedback loop ───────────────────────────────────────────────

	[Fact]
	public void TwoWay_no_feedback_loop()
	{
		var vm = new ViewModel { Text = "start" };
		var el = new TestElement();
		el.DataContext = vm;
		el.SetBinding(TestElement.ValueProperty,
			new Binding("Text") { Mode = BindingMode.TwoWay });

		int inpcCount = 0;
		vm.PropertyChanged += (_, _) => inpcCount++;

		// Set via DP — write-back fires INPC once, which re-pushes to target.
		// Without the guard this would loop.
		el.Value = "loop-test";

		// INPC fires exactly once (the write-back): no infinite loop.
		Assert.Equal(1, inpcCount);
		Assert.Equal("loop-test", vm.Text);
	}

	// ── ConvertBack on TwoWay write-back ─────────────────────────────────────

	[Fact]
	public void TwoWay_ConvertBack_applied_on_writeback()
	{
		var vm = new ViewModel { Text = "hello" };
		var el = new TestElement();
		el.DataContext = vm;
		el.SetBinding(TestElement.ValueProperty,
			new Binding("Text") { Mode = BindingMode.TwoWay, Converter = new SuffixConverter() });

		// Forward: "hello" -> "hello_fwd"
		Assert.Equal("hello_fwd", el.Value);

		// Write-back: "world_fwd" -> ConvertBack -> "world"
		el.Value = "world_fwd";
		Assert.Equal("world", vm.Text);
	}

	// ── OneTime ──────────────────────────────────────────────────────────────

	[Fact]
	public void OneTime_pushes_initial_value()
	{
		var vm = new ViewModel { Text = "once" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Text") { Mode = BindingMode.OneTime });

		Assert.Equal("once", tb.Text);
	}

	[Fact]
	public void OneTime_does_not_update_after_source_changes()
	{
		var vm = new ViewModel { Text = "first" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Text") { Mode = BindingMode.OneTime });

		vm.Text = "second";
		Assert.Equal("first", tb.Text); // must not have updated
	}

	// ── ElementName resolution ────────────────────────────────────────────────

	[Fact]
	public void ElementName_resolves_named_sibling_as_source()
	{
		// source element registered in a name scope on the root.
		var root = new TestElement();
		var source = new TestElement { Value = "from-sibling" };
		root.RegisterName("sibling", source);

		// target element: FindName walks up to root via InheritanceParent.
		var target = new TestElement();
		target.InheritanceParent = root;

		target.SetBinding(TestElement.ValueProperty,
			new Binding("Value") { ElementName = "sibling" });

		Assert.Equal("from-sibling", target.Value);
	}

	[Fact]
	public void ElementName_updates_target_when_source_dp_changes()
	{
		var root = new TestElement();
		var source = new TestElement { Value = "initial" };
		root.RegisterName("src", source);

		var target = new TestElement();
		target.InheritanceParent = root;
		target.SetBinding(TestElement.ValueProperty,
			new Binding("Value") { ElementName = "src" });

		Assert.Equal("initial", target.Value);

		// Round 36: a DependencyProperty source is watched like WPF does (no INPC needed),
		// so changing the source DP re-pushes the target.
		source.Value = "changed";
		// WPF-faithful: the ElementName target follows the source DP.
		Assert.Equal("changed", target.Value);
	}
}