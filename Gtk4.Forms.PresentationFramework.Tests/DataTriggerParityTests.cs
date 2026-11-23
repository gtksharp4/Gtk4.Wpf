using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>DataTrigger / MultiDataTrigger honour RelativeSource, ElementName, converters and WPF's comparison.</summary>
public class DataTriggerParityTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		int _count;
		string? _name;
		bool _flag;
		public int Count { get => _count; set { _count = value; Raise(nameof(Count)); } }
		public string? Name { get => _name; set { _name = value; Raise(nameof(Name)); } }
		public bool Flag { get => _flag; set { _flag = value; Raise(nameof(Flag)); } }
		public event PropertyChangedEventHandler? PropertyChanged;
		void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("TriggerOut", typeof(string), typeof(TEl), new PropertyMetadata(null));
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class GreaterThan : IValueConverter
	{
		public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture) =>
			(int)value! > int.Parse((string)parameter!, CultureInfo.InvariantCulture);

		public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture) =>
			throw new System.NotSupportedException();
	}

	static Style StyleWith(Binding binding, object? value)
	{
		var style = new Style(typeof(TEl));
		var trigger = new DataTrigger { Binding = binding, Value = value };
		trigger.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "on" });
		style.Triggers.Add(trigger);
		return style;
	}

	[Fact]
	public void String_Value_matches_a_boxed_int_and_follows_the_source() => Run(() =>
	{
		var vm = new Vm { Count = 2 };
		var el = new TEl { DataContext = vm, Style = StyleWith(new Binding("Count"), "2") };
		Assert.Equal("on", el.Out);
		vm.Count = 3;
		Assert.Null(el.Out);
		vm.Count = 2;
		Assert.Equal("on", el.Out);
	});

	[Fact]
	public void String_True_matches_a_bool() => Run(() =>
	{
		var vm = new Vm();
		var el = new TEl { DataContext = vm, Style = StyleWith(new Binding("Flag"), "True") };
		Assert.Null(el.Out);
		vm.Flag = true;
		Assert.Equal("on", el.Out);
	});

	[Fact]
	public void Null_Value_matches_a_null_source_value_only() => Run(() =>
	{
		var vm = new Vm();
		var el = new TEl { DataContext = vm, Style = StyleWith(new Binding("Name"), null) };
		Assert.Equal("on", el.Out);
		vm.Name = "x";
		Assert.Null(el.Out);
	});

	[Fact]
	public void Converter_is_applied_before_the_comparison() => Run(() =>
	{
		var vm = new Vm { Count = 1 };
		var binding = new Binding("Count") { Converter = new GreaterThan(), ConverterParameter = "5" };
		var el = new TEl { DataContext = vm, Style = StyleWith(binding, true) };
		Assert.Null(el.Out);
		vm.Count = 9;
		Assert.Equal("on", el.Out);
		vm.Count = 2;
		Assert.Null(el.Out);
	});

	[Fact]
	public void RelativeSource_Self_reads_the_element_itself() => Run(() =>
	{
		var el = new TEl { Tag = "a" };
		el.Style = StyleWith(new Binding("Tag") { RelativeSource = RelativeSource.Self }, "b");
		Assert.Null(el.Out);
		el.Tag = "b";
		Assert.Equal("on", el.Out);
		el.Tag = "c";
		Assert.Null(el.Out);
	});

	[Fact]
	public void RelativeSource_FindAncestor_resolves_once_the_element_is_in_the_tree() => Run(() =>
	{
		var el = new TEl();
		el.Style = StyleWith(new Binding("Tag")
		{
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Border), 1)
		}, "go");
		Assert.Null(el.Out);

		var border = new Border { Tag = "go", Child = el };

		Assert.Equal("on", el.Out);
		border.Tag = "stop";
		Assert.Null(el.Out);
	});

	[Fact]
	public void ElementName_resolves_when_the_name_is_registered_later() => Run(() =>
	{
		var root = new StackPanel();
		var source = new Border { Tag = "go" };
		var el = new TEl { Style = StyleWith(new Binding("Tag") { ElementName = "src" }, "go") };
		root.Children.Add(source);
		root.Children.Add(el);
		Assert.Null(el.Out);

		root.RegisterName("src", source);

		Assert.Equal("on", el.Out);
		source.Tag = "stop";
		Assert.Null(el.Out);
		source.Tag = "go";
		Assert.Equal("on", el.Out);
	});

	[Fact]
	public void Attached_property_path_on_a_DependencyObject_DataContext_is_walked_and_watched() => Run(() =>
	{
		var holder = new Border();
		Grid.SetRow(holder, 1);
		var el = new TEl { DataContext = holder, Style = StyleWith(new Binding("(Grid.Row)"), "1") };
		Assert.Equal("on", el.Out);
		Grid.SetRow(holder, 2);
		Assert.Null(el.Out);
		Grid.SetRow(holder, 1);
		Assert.Equal("on", el.Out);
	});

	[Fact]
	public void MultiDataTrigger_conditions_use_RelativeSource_and_string_comparison() => Run(() =>
	{
		var vm = new Vm { Count = 4 };
		var el = new TEl { DataContext = vm, Tag = "t" };
		var style = new Style(typeof(TEl));
		var trigger = new MultiDataTrigger();
		trigger.Conditions.Add(new Condition { Binding = new Binding("Count"), Value = "4" });
		trigger.Conditions.Add(new Condition
		{
			Binding = new Binding("Tag") { RelativeSource = RelativeSource.Self },
			Value = "t"
		});
		trigger.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "on" });
		style.Triggers.Add(trigger);
		el.Style = style;
		Assert.Equal("on", el.Out);
		el.Tag = "u";
		Assert.Null(el.Out);
		el.Tag = "t";
		vm.Count = 5;
		Assert.Null(el.Out);
	});
}
