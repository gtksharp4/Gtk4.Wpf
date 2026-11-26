using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>BindingGroup.GetValue / TryGetValue and ItemsControl.ContainerFromElement.</summary>
public class BindingGroupValueTests
{
	sealed class Vm
	{
		public double Price { get; set; } = 1.5;
		public string Name { get; set; } = "n";
	}

	sealed class TextElement : FrameworkElement
	{
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextElement), new PropertyMetadata(""));

		public string Text
		{
			get => (string)GetValue(TextProperty)!;
			set => SetValue(TextProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class Doubler : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
			double.Parse((string)value!, CultureInfo.InvariantCulture) * 2;
	}

	static (BindingGroup Group, Vm Vm, TextElement Price, TextElement Name) Build(IValueConverter? converter = null)
	{
		var vm = new Vm();
		var price = new TextElement { DataContext = vm };
		var name = new TextElement { DataContext = vm };
		var priceExpr = (BindingExpression)price.SetBinding(TextElement.TextProperty, new Binding("Price")
		{
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
			Converter = converter,
			ConverterCulture = CultureInfo.InvariantCulture,
		})!;
		var nameExpr = (BindingExpression)name.SetBinding(TextElement.TextProperty, new Binding("Name")
		{
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
		})!;
		var group = new BindingGroup();
		group.BindingExpressions.Add(priceExpr);
		group.BindingExpressions.Add(nameExpr);
		return (group, vm, price, name);
	}

	[Fact]
	public void TryGetValue_returns_the_proposed_value_converted_to_the_source_type()
	{
		var (group, vm, price, name) = Build();
		price.Text = "42.5";
		name.Text = "changed";
		Assert.True(group.TryGetValue(vm, "Price", out var proposed));
		Assert.Equal(42.5, proposed);
		Assert.True(group.TryGetValue(vm, "Name", out var text));
		Assert.Equal("changed", text);
		Assert.Equal(1.5, vm.Price);
	}

	[Fact]
	public void GetValue_applies_the_converter_back()
	{
		var (group, vm, price, _) = Build(new Doubler());
		price.Text = "4";
		Assert.Equal(8.0, group.GetValue(vm, "Price"));
	}

	[Fact]
	public void Unknown_item_or_property_is_unavailable()
	{
		var (group, vm, _, _) = Build();
		Assert.False(group.TryGetValue(vm, "Missing", out var value));
		Assert.Null(value);
		Assert.False(group.TryGetValue(new Vm(), "Price", out _));
		Assert.Throws<ValueUnavailableException>(() => group.GetValue(vm, "Missing"));
		Assert.Throws<ValueUnavailableException>(() => group.GetValue(new Vm(), "Price"));
	}

	[Fact]
	public void A_proposed_value_that_cannot_be_converted_is_unavailable()
	{
		var (group, vm, price, _) = Build();
		price.Text = "not a number";
		Assert.False(group.TryGetValue(vm, "Price", out _));
	}

	[Fact]
	public void Arguments_are_validated()
	{
		var (group, vm, _, _) = Build();
		Assert.Throws<ArgumentNullException>(() => group.GetValue(null!, "Price"));
		Assert.Throws<ArgumentNullException>(() => group.GetValue(vm, null!));
		Assert.Throws<ArgumentNullException>(() => group.TryGetValue(vm, null!, out _));
	}

	[Fact]
	public void Group_rules_receive_the_group_as_the_value()
	{
		var (group, _, price, _) = Build();
		object? seen = null;
		group.ValidationRules.Add(new CaptureRule(v => seen = v));
		price.Text = "3";
		Assert.True(group.CommitEdit());
		Assert.Same(group, seen);
	}

	sealed class CaptureRule : ValidationRule
	{
		readonly Action<object?> _capture;

		public CaptureRule(Action<object?> capture) => _capture = capture;

		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		{
			_capture(value);
			return ValidationResult.ValidResult;
		}
	}
}
