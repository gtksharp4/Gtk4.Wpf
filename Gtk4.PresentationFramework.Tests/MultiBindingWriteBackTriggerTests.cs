using System;
using System.Globalization;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>MultiBinding honours the resolved UpdateSourceTrigger and survives a throwing ConvertBack.</summary>
public class MultiBindingWriteBackTriggerTests : WpfGtkTestBase
{
	sealed class Vm
	{
		public string First { get; set; } = "a";
	}

	sealed class CountingConverter : IMultiValueConverter
	{
		public int ConvertBackCalls;
		public bool Throw;

		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> values[0];

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
		{
			ConvertBackCalls++;
			if (Throw) throw new NotImplementedException();
			return new[] { value };
		}
	}

	static WpfTextBox Bind(Vm vm, CountingConverter converter)
	{
		var tb = new WpfTextBox { DataContext = vm };
		var mb = new MultiBinding { Converter = converter };
		mb.Bindings.Add(new Binding("First"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);
		return tb;
	}

	[Fact]
	public void TextBox_text_defers_ConvertBack_until_LostFocus() => Run(() =>
	{
		var vm = new Vm();
		var converter = new CountingConverter();
		var tb = Bind(vm, converter);
		tb.Focus();

		tb.Text = "typed";
		Assert.Equal(0, converter.ConvertBackCalls);
		Assert.Equal("a", vm.First);

		new WpfTextBox().Focus();

		Assert.Equal(1, converter.ConvertBackCalls);
		Assert.Equal("typed", vm.First);
	});

	[Fact]
	public void Throwing_ConvertBack_does_not_escape_LostFocus() => Run(() =>
	{
		var vm = new Vm();
		var converter = new CountingConverter { Throw = true };
		var tb = Bind(vm, converter);
		tb.Focus();

		tb.Text = "typed";
		Assert.Equal(0, converter.ConvertBackCalls);
		var ex = Record.Exception(() => new WpfTextBox().Focus());

		Assert.Null(ex);
		Assert.Equal(1, converter.ConvertBackCalls);
		Assert.Equal("a", vm.First);
	});

	[Fact]
	public void Throwing_ConvertBack_does_not_escape_the_Text_setter_with_PropertyChanged_trigger() => Run(() =>
	{
		var vm = new Vm();
		var converter = new CountingConverter { Throw = true };
		var tb = new WpfTextBox { DataContext = vm };
		var mb = new MultiBinding { Converter = converter, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
		mb.Bindings.Add(new Binding("First"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);

		var ex = Record.Exception(() => tb.Text = "typed");

		Assert.Null(ex);
		Assert.Equal(1, converter.ConvertBackCalls);
		Assert.Equal("a", vm.First);
	});

	[Fact]
	public void Explicit_trigger_never_writes_automatically() => Run(() =>
	{
		var vm = new Vm();
		var converter = new CountingConverter();
		var tb = new WpfTextBox { DataContext = vm };
		var mb = new MultiBinding { Converter = converter, UpdateSourceTrigger = UpdateSourceTrigger.Explicit };
		mb.Bindings.Add(new Binding("First"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);

		tb.Text = "typed";
		tb.Focus();
		new WpfTextBox().Focus();

		Assert.Equal(0, converter.ConvertBackCalls);
	});

	[Fact]
	public void PropertyChanged_trigger_writes_on_every_change() => Run(() =>
	{
		var vm = new Vm();
		var converter = new CountingConverter();
		var tb = new WpfTextBox { DataContext = vm };
		var mb = new MultiBinding { Converter = converter, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
		mb.Bindings.Add(new Binding("First"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);

		tb.Text = "typed";

		Assert.Equal(1, converter.ConvertBackCalls);
		Assert.Equal("typed", vm.First);
	});
}
