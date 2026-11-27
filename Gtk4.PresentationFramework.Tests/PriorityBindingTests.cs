using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="PriorityBinding"/>.
/// Each test fails without the feature and passes with it.
/// </summary>
public class PriorityBindingTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	sealed class PriorityElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(PriorityElement),
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
		string? _name;
		public string? Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

	[Fact]
	public void PriorityBinding_uses_first_non_null_child()
	{
		var vm1 = new SourceVm { Name = null };
		var vm2 = new SourceVm { Name = "second" };

		var el = new PriorityElement();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });

		el.SetBinding(PriorityElement.ValueProperty, pb);

		// First child resolves to null, so second is used.
		Assert.Equal("second", el.Value);
	}

	[Fact]
	public void PriorityBinding_uses_first_child_when_it_is_non_null()
	{
		var vm1 = new SourceVm { Name = "first" };
		var vm2 = new SourceVm { Name = "second" };

		var el = new PriorityElement();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });

		el.SetBinding(PriorityElement.ValueProperty, pb);

		Assert.Equal("first", el.Value);
	}

	[Fact]
	public void PriorityBinding_FallbackValue_used_when_all_null()
	{
		var vm1 = new SourceVm { Name = null };
		var vm2 = new SourceVm { Name = null };

		var el = new PriorityElement();
		var pb = new PriorityBinding { FallbackValue = "fallback" };
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });

		el.SetBinding(PriorityElement.ValueProperty, pb);

		Assert.Equal("fallback", el.Value);
	}

	[Fact]
	public void PriorityBinding_re_evaluates_on_source_change()
	{
		var vm1 = new SourceVm { Name = null };
		var vm2 = new SourceVm { Name = "second" };

		var el = new PriorityElement();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });

		el.SetBinding(PriorityElement.ValueProperty, pb);
		Assert.Equal("second", el.Value);

		// When vm1 gets a value, it should take priority.
		vm1.Name = "first-now";
		Assert.Equal("first-now", el.Value);
	}

	[Fact]
	public void PriorityBinding_expression_returned_by_SetBinding()
	{
		var vm = new SourceVm { Name = "x" };
		var el = new PriorityElement();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm });

		var expr = el.SetBinding(PriorityElement.ValueProperty, pb);

		Assert.IsType<PriorityBindingExpression>(expr);
	}

	[Fact]
	public void PriorityBinding_is_re_evaluated_when_the_inherited_DataContext_arrives_later()
	{
		var parent = new PriorityElement();
		var el = new PriorityElement();
		el.InheritanceParent = parent;
		var pb = new PriorityBinding { FallbackValue = "fallback" };
		pb.Bindings.Add(new Binding("Name"));
		el.SetBinding(PriorityElement.ValueProperty, pb);
		Assert.Equal("fallback", el.Value);

		parent.DataContext = new SourceVm { Name = "from-context" };

		Assert.Equal("from-context", el.Value);
	}

	[Fact]
	public void PriorityBinding_is_re_evaluated_when_the_local_DataContext_is_replaced()
	{
		var el = new PriorityElement();
		var pb = new PriorityBinding { FallbackValue = "fallback" };
		pb.Bindings.Add(new Binding("Name"));
		el.SetBinding(PriorityElement.ValueProperty, pb);

		el.DataContext = new SourceVm { Name = "one" };
		Assert.Equal("one", el.Value);
		el.DataContext = new SourceVm { Name = "two" };
		Assert.Equal("two", el.Value);
	}
}
