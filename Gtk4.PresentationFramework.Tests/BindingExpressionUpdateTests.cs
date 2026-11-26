using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>BindingExpression(Base).UpdateSource/ParentBinding and the SourceUpdated/TargetUpdated events.</summary>
public class BindingExpressionUpdateTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		string _name = "n";
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public string First { get; set; } = "";
		public string Last { get; set; } = "";
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class Splitter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
			values[0] + " " + values[1];

		public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
		{
			var parts = ((string)value!).Split(' ');
			return new object?[] { parts[0], parts[1] };
		}
	}

	[Fact]
	public void GetBindingExpression_returns_a_BindingExpression_with_its_ParentBinding() => Run(() =>
	{
		var binding = new Binding("Name") { UpdateSourceTrigger = UpdateSourceTrigger.Explicit };
		var tb = new WpfTextBox { DataContext = new Vm() };
		tb.SetBinding(WpfTextBox.TextProperty, binding);

		BindingExpression? be = tb.GetBindingExpression(WpfTextBox.TextProperty);
		Assert.NotNull(be);
		Assert.Same(binding, be!.ParentBinding);
		Assert.Same(binding, ((BindingExpressionBase)be).ParentBindingBase);
	});

	[Fact]
	public void GetBindingExpression_is_null_for_an_unbound_property() => Run(() =>
	{
		Assert.Null(new WpfTextBox().GetBindingExpression(WpfTextBox.TextProperty));
	});

	[Fact]
	public void BindingExpressionBase_UpdateSource_writes_the_target_value_back() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		BindingExpressionBase expr = tb.SetBinding(WpfTextBox.TextProperty,
			(BindingBase)new Binding("Name") { UpdateSourceTrigger = UpdateSourceTrigger.Explicit })!;
		tb.Text = "typed";
		Assert.Equal("n", vm.Name);
		expr.UpdateSource();
		Assert.Equal("typed", vm.Name);
	});

	[Fact]
	public void BindingExpressionBase_UpdateTarget_repulls_the_source() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBlock { DataContext = vm };
		BindingExpressionBase expr = tb.SetBinding(WpfTextBlock.TextProperty, (BindingBase)new Binding("Name"))!;
		vm.First = "x";
		expr.UpdateTarget();
		Assert.Equal("n", tb.Text);
	});

	[Fact]
	public void MultiBindingExpression_UpdateSource_runs_ConvertBack() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		var mb = new MultiBinding { Converter = new Splitter(), Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.Explicit };
		mb.Bindings.Add(new Binding("First"));
		mb.Bindings.Add(new Binding("Last"));
		var expr = tb.SetBinding(WpfTextBox.TextProperty, mb)!;
		tb.Text = "Ann Jones";
		expr.UpdateSource();
		Assert.Equal("Ann", vm.First);
		Assert.Equal("Jones", vm.Last);
		Assert.Same(mb, expr.ParentBindingBase);
	});

	[Fact]
	public void DataContextChanged_is_raised_with_old_and_new_values() => Run(() =>
	{
		var tb = new WpfTextBlock();
		DependencyPropertyChangedEventArgs? seen = null;
		tb.DataContextChanged += (_, e) => seen = e;
		tb.DataContext = "x";
		Assert.Equal("x", seen!.Value.NewValue);
		Assert.Null(seen.Value.OldValue);
	});

	[Fact]
	public void TargetUpdated_is_raised_only_with_NotifyOnTargetUpdated() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBlock { DataContext = vm };
		DataTransferEventArgs? seen = null;
		var count = 0;
		tb.TargetUpdated += (_, e) => { seen = e; count++; };
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Name") { NotifyOnTargetUpdated = true });
		Assert.Equal(1, count);
		Assert.Same(tb, seen!.TargetObject);
		Assert.Same(WpfTextBlock.TextProperty, seen.Property);
		Assert.Same(Binding.TargetUpdatedEvent, seen.RoutedEvent);

		vm.Name = "changed";
		Assert.Equal(2, count);

		var quiet = new WpfTextBlock { DataContext = vm };
		var quietCount = 0;
		quiet.TargetUpdated += (_, _) => quietCount++;
		quiet.SetBinding(WpfTextBlock.TextProperty, new Binding("Name"));
		Assert.Equal(0, quietCount);
	});

	[Fact]
	public void SourceUpdated_is_raised_after_the_value_reaches_the_source() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		string? atEvent = null;
		tb.SourceUpdated += (_, e) =>
		{
			atEvent = vm.Name;
			Assert.Same(WpfTextBox.TextProperty, e.Property);
		};
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name")
		{
			UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
			NotifyOnSourceUpdated = true
		});
		tb.Text = "new";
		Assert.Null(atEvent);
		tb.GetBindingExpression(WpfTextBox.TextProperty)!.UpdateSource();
		Assert.Equal("new", atEvent);
	});

	[Fact]
	public void Attached_handlers_receive_the_events() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBlock { DataContext = vm };
		var count = 0;
		EventHandler<DataTransferEventArgs> handler = (_, _) => count++;
		Binding.AddTargetUpdatedHandler(tb, handler);
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Name") { NotifyOnTargetUpdated = true });
		Binding.RemoveTargetUpdatedHandler(tb, handler);
		vm.Name = "x";
		Assert.Equal(1, count);
	});
}
