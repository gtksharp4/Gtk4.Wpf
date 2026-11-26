using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="BindingOperations"/>.
/// Each test fails without the feature and passes with it.
/// </summary>
public class BindingOperationsTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

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

	sealed class SourceVm : INotifyPropertyChanged
	{
		string _name = "initial";
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

	[Fact]
	public void SetBinding_returns_BindingExpression()
	{
		var vm = new SourceVm();
		var el = new TestElement { DataContext = vm };
		var expr = BindingOperations.SetBinding(
			el, TestElement.ValueProperty, new Binding("Name"));

		Assert.IsType<BindingExpression>(expr);
	}

	[Fact]
	public void SetBinding_binding_is_active()
	{
		var vm = new SourceVm { Name = "hello" };
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));

		Assert.Equal("hello", el.Value);
	}

	[Fact]
	public void GetBindingExpression_returns_expression_after_SetBinding()
	{
		var vm = new SourceVm();
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));

		var expr = BindingOperations.GetBindingExpression(el, TestElement.ValueProperty);
		Assert.NotNull(expr);
	}

	[Fact]
	public void GetBinding_returns_binding_object()
	{
		var vm = new SourceVm();
		var el = new TestElement { DataContext = vm };
		var original = new Binding("Name");
		BindingOperations.SetBinding(el, TestElement.ValueProperty, original);

		var retrieved = BindingOperations.GetBinding(el, TestElement.ValueProperty);
		Assert.Same(original, retrieved);
	}

	[Fact]
	public void ClearBinding_removes_active_binding()
	{
		var vm = new SourceVm { Name = "alive" };
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));
		Assert.Equal("alive", el.Value);

		BindingOperations.ClearBinding(el, TestElement.ValueProperty);

		// After clear the expression is gone.
		Assert.Null(BindingOperations.GetBindingExpression(el, TestElement.ValueProperty));

		// Source changes no longer update target.
		vm.Name = "dead";
		Assert.Equal("alive", el.Value); // unchanged
	}

	[Fact]
	public void IsDataBound_true_after_SetBinding()
	{
		var vm = new SourceVm();
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));

		Assert.True(BindingOperations.IsDataBound(el, TestElement.ValueProperty));
	}

	[Fact]
	public void IsDataBound_false_after_ClearBinding()
	{
		var vm = new SourceVm();
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));
		BindingOperations.ClearBinding(el, TestElement.ValueProperty);

		Assert.False(BindingOperations.IsDataBound(el, TestElement.ValueProperty));
	}

	[Fact]
	public void ClearAllBindings_removes_all_bindings()
	{
		var vm = new SourceVm { Name = "y" };
		var el = new TestElement { DataContext = vm };
		BindingOperations.SetBinding(el, TestElement.ValueProperty, new Binding("Name"));
		BindingOperations.ClearAllBindings(el);

		Assert.False(BindingOperations.IsDataBound(el, TestElement.ValueProperty));
	}

	[Fact]
	public void GetMultiBindingExpression_returns_expression_for_MultiBinding()
	{
		var vm = new SourceVm { Name = "z" };
		var el = new TestElement { DataContext = vm };
		var mb = new MultiBinding();
		mb.Bindings.Add(new Binding("Name"));
		BindingOperations.SetBinding(el, TestElement.ValueProperty, mb);

		var expr = BindingOperations.GetMultiBindingExpression(el, TestElement.ValueProperty);
		Assert.NotNull(expr);
		Assert.IsType<MultiBindingExpression>(expr);
	}

	[Fact]
	public void SetBinding_PriorityBinding_round_trip()
	{
		var vm1 = new SourceVm { Name = null! };
		var vm2 = new SourceVm { Name = "fallthrough" };
		var el = new TestElement();

		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });

		BindingOperations.SetBinding(el, TestElement.ValueProperty, pb);
		Assert.Equal("fallthrough", el.Value);

		// Expression is accessible.
		Assert.NotNull(BindingOperations.GetBindingExpressionBase(el, TestElement.ValueProperty));
	}
}
