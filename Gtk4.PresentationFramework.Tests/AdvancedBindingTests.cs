using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Round-7 pure tests: RelativeSource, MultiBinding, BindingExpression,
/// StringFormat, FallbackValue, TargetNullValue, UpdateSourceTrigger.
/// Each test fails without the matching feature.
/// </summary>
public class AdvancedBindingTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	sealed class ViewModel : INotifyPropertyChanged
	{
		string _first = string.Empty;
		string _last = string.Empty;

		public string First
		{
			get => _first;
			set { if (_first == value) return; _first = value; RaiseChanged(nameof(First)); }
		}

		public string Last
		{
			get => _last;
			set { if (_last == value) return; _last = value; RaiseChanged(nameof(Last)); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		void RaiseChanged(string name) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	class AncestorElement : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class DescendantElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(DescendantElement),
				new PropertyMetadata(null));
		public string? Value
		{
			get => (string?)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class ConcatConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter,
			CultureInfo culture)
			=> string.Join(" ", values);

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter,
			CultureInfo culture) => null;
	}

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
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	// ── RelativeSource.Self ────────────────────────────────────────────────

	[Fact]
	public void RelativeSource_Self_binds_own_DataContext()
	{
		var tb = new WpfTextBlock();
		tb.DataContext = "ctx-val";
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("DataContext")
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.Self)
			});
		// TextBlock.Text should equal the TextBlock's own DataContext.
		Assert.Equal("ctx-val", tb.Text);
	}

	[Fact]
	public void RelativeSource_Self_initial_push_on_set()
	{
		var tb = new WpfTextBlock();
		tb.DataContext = "initial";
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("DataContext")
			{
				RelativeSource = RelativeSource.Self
			});
		Assert.Equal("initial", tb.Text);
	}

	// ── RelativeSource.FindAncestor ────────────────────────────────────────

	[Fact]
	public void RelativeSource_FindAncestor_walks_to_typed_ancestor()
	{
		var ancestor = new AncestorElement { Tag = "ancestor-value" };
		var child = new DescendantElement();
		child.LogicalParent = ancestor;

		child.SetBinding(DescendantElement.ValueProperty,
			new Binding("Tag")
			{
				RelativeSource = new RelativeSource(
					RelativeSourceMode.FindAncestor,
					typeof(AncestorElement),
					1)
			});

		Assert.Equal("ancestor-value", child.Value);
	}

	[Fact]
	public void RelativeSource_FindAncestor_level2_skips_first_match()
	{
		var grandParent = new AncestorElement { Tag = "grandparent" };
		var midParent = new AncestorElement { Tag = "parent" };
		var child = new DescendantElement();

		midParent.LogicalParent = grandParent;
		child.LogicalParent = midParent;

		child.SetBinding(DescendantElement.ValueProperty,
			new Binding("Tag")
			{
				RelativeSource = new RelativeSource(
					RelativeSourceMode.FindAncestor,
					typeof(AncestorElement),
					2)   // 2nd ancestor of type AncestorElement = grandParent
			});

		Assert.Equal("grandparent", child.Value);
	}

	// ── MultiBinding ──────────────────────────────────────────────────────

	[Fact]
	public void MultiBinding_combines_two_sources()
	{
		var vm = new ViewModel { First = "John", Last = "Doe" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;

		var mb = new MultiBinding { Converter = new ConcatConverter() };
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);

		Assert.Equal("John Doe", tb.Text);
	}

	[Fact]
	public void MultiBinding_updates_on_child_source_change()
	{
		var vm = new ViewModel { First = "Jane", Last = "Smith" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;

		var mb = new MultiBinding { Converter = new ConcatConverter() };
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);

		vm.First = "Alice";
		Assert.Equal("Alice Smith", tb.Text);
	}

	[Fact]
	public void MultiBinding_StringFormat_applied()
	{
		var vm = new ViewModel { First = "Ann", Last = "Jones" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;

		var mb = new MultiBinding
		{
			Converter = new ConcatConverter(),
			StringFormat = "Name: {0}"
		};
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);

		Assert.Equal("Name: Ann Jones", tb.Text);
	}

	// ── BindingExpression live object ─────────────────────────────────────

	[Fact]
	public void GetBindingExpression_returns_non_null_after_SetBinding()
	{
		var vm = new ViewModel { First = "X" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("First"));

		var expr = tb.GetBindingExpression(WpfTextBlock.TextProperty);
		Assert.NotNull(expr);
	}

	[Fact]
	public void BindingExpression_UpdateTarget_repulls_value()
	{
		var vm = new ViewModel { First = "Before" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		var expr = (BindingExpression?)tb.SetBinding(
			WpfTextBlock.TextProperty, new Binding("First"));

		Assert.NotNull(expr);
		Assert.Equal("Before", tb.Text);

		// UpdateTarget forces a re-pull from the current source.
		vm.First = "After";  // INPC fires → auto-push
		Assert.Equal("After", tb.Text);

		vm.First = "Direct";
		expr!.UpdateTarget();
		Assert.Equal("Direct", tb.Text);
	}

	[Fact]
	public void BindingExpression_ResolvedSource_is_DataContext()
	{
		var vm = new ViewModel { First = "src" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		var expr = (BindingExpression?)tb.SetBinding(
			WpfTextBlock.TextProperty, new Binding("First"));

		Assert.Same(vm, expr!.ResolvedSource);
	}

	// ── StringFormat ──────────────────────────────────────────────────────

	[Fact]
	public void StringFormat_with_placeholder_formats_value()
	{
		var vm = new ViewModel { First = "World" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("First") { StringFormat = "Hello {0}!" });

		Assert.Equal("Hello World!", tb.Text);
	}

	[Fact]
	public void StringFormat_without_braces_is_a_format_item_body()
	{
		var vm = new ViewModel { First = "World" };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("First") { StringFormat = "Greeting: " });

		// WPF GetEffectiveStringFormat: no brace → "{0:Greeting: }"; a string ignores the custom format.
		Assert.Equal("World", tb.Text);
	}

	// ── FallbackValue ─────────────────────────────────────────────────────

	[Fact]
	public void FallbackValue_used_when_path_cannot_resolve()
	{
		var tb = new WpfTextBlock();
		// No DataContext → source is null → path unresolvable.
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("NonExistentPath") { FallbackValue = "fallback" });

		Assert.Equal("fallback", tb.Text);
	}

	// ── TargetNullValue ───────────────────────────────────────────────────

	sealed class NullableViewModel
	{
		public string? NullableProp { get; set; } = null;
	}

	[Fact]
	public void TargetNullValue_substituted_when_resolved_value_is_null()
	{
		var vm = new NullableViewModel { NullableProp = null };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("NullableProp")
			{
				TargetNullValue = "null-placeholder"
			});
		Assert.Equal("null-placeholder", tb.Text);
	}

	// ── UpdateSourceTrigger ───────────────────────────────────────────────

	[Fact]
	public void UpdateSourceTrigger_Explicit_does_not_auto_writeback()
	{
		var vm = new ViewModel { First = "initial" };
		var el = new TestElement();
		el.DataContext = vm;
		var expr = (BindingExpression?)el.SetBinding(
			TestElement.ValueProperty,
			new Binding("First")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit
			});

		Assert.Equal("initial", el.Value);

		// Mutate target DP — source should NOT be updated automatically.
		el.Value = "changed";
		Assert.Equal("initial", vm.First); // Source unchanged.

		// Explicitly update: now source reflects the target.
		expr!.UpdateSource();
		Assert.Equal("changed", vm.First);
	}

	[Fact]
	public void UpdateSourceTrigger_PropertyChanged_auto_writes_back()
	{
		var vm = new ViewModel { First = "start" };
		var el = new TestElement();
		el.DataContext = vm;
		el.SetBinding(TestElement.ValueProperty,
			new Binding("First")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

		el.Value = "updated";
		Assert.Equal("updated", vm.First);
	}
}