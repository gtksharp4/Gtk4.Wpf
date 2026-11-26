using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 45 review fixes: converter hooks, control members and current-item bindings.</summary>
public class Round45FixTests : WpfGtkTestBase
{
	public class Division { public string Name { get; set; } = ""; }

	public class Company { public List<Division> Divisions { get; set; } = new(); }

	public class Record
	{
		public string EmployeeNumber { get; set; } = "";
		public int Count { get; set; }
		public DayOfWeek Day { get; set; }
	}

	[ValueConversion(typeof(int), typeof(string))]
	[ValueConversion(typeof(bool), typeof(string))]
	sealed class TwoWayConverter { }

	[Fact]
	public void SetClrProperty_keeps_a_string_and_converts_numbers_and_enums()
	{
		var r = new Record();
		XamlValueConverter.SetClrProperty(r, "EmployeeNumber", "57304");
		XamlValueConverter.SetClrProperty(r, "Count", "3");
		XamlValueConverter.SetClrProperty(r, "Day", "friday");
		Assert.Equal("57304", r.EmployeeNumber);
		Assert.Equal(3, r.Count);
		Assert.Equal(DayOfWeek.Friday, r.Day);
		Assert.Throws<InvalidOperationException>(() => XamlValueConverter.SetClrProperty(r, "Nope", "1"));
	}

	[Fact]
	public void SetClrValue_converts_resource_text_to_the_property_type()
	{
		var r = new Record();
		XamlValueConverter.SetClrValue(r, "Count", "7");
		XamlValueConverter.SetClrValue(r, "EmployeeNumber", "x");
		Assert.Equal(7, r.Count);
		Assert.Equal("x", r.EmployeeNumber);
		Assert.Equal(System.Windows.Media.Colors.Red,
			XamlValueConverter.CoerceResource(typeof(System.Windows.Media.Color), "Red"));
	}

	[Fact]
	public void ObjectDataProvider_with_a_typeof_parameter_yields_the_enum_values()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Enum), MethodName = "GetValues" };
		odp.MethodParameters.Add(typeof(DayOfWeek));
		odp.Refresh();
		Assert.Equal(7, ((Array)odp.Data!).Length);
	}

	[Fact]
	public void ValueConversion_can_be_applied_twice()
	{
		var attrs = (ValueConversionAttribute[])typeof(TwoWayConverter)
			.GetCustomAttributes(typeof(ValueConversionAttribute), false);
		Assert.Equal(2, attrs.Length);
		Assert.Contains(attrs, a => a.SourceType == typeof(int) && a.TargetType == typeof(string));
		Assert.Contains(attrs, a => a.SourceType == typeof(bool) && a.TargetType == typeof(string));
	}

	[Fact]
	public void ContentPresenter_ContentSource_is_a_dependency_property() => Run(() =>
	{
		var cp = new ContentPresenter();
		Assert.Equal("Content", cp.GetValue(ContentPresenter.ContentSourceProperty));
		cp.ContentSource = "Header";
		Assert.Equal("Header", cp.GetValue(ContentPresenter.ContentSourceProperty));
		cp.SetValue(ContentPresenter.ContentSourceProperty, "Footer");
		Assert.Equal("Footer", cp.ContentSource);
	});

	[Fact]
	public void SpellingReform_values_match_WPF()
	{
		Assert.Equal(0, (int)SpellingReform.PreAndPostreform);
		Assert.Equal(1, (int)SpellingReform.Prereform);
		Assert.Equal(2, (int)SpellingReform.Postreform);
	}

	[Fact]
	public void SpellCheck_GetSpellingReform_is_not_public_and_the_instance_getter_reads_the_value() => Run(() =>
	{
		Assert.Null(typeof(SpellCheck).GetMethod("GetSpellingReform",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
		var tb = new TextBox();
		SpellCheck.SetSpellingReform(tb, SpellingReform.Postreform);
		Assert.Equal(SpellingReform.Postreform, tb.SpellCheck.SpellingReform);
	});

	[Fact]
	public void HasContent_is_get_only_and_follows_Content() => Run(() =>
	{
		Assert.Null(typeof(ContentControl).GetProperty("HasContent")!.GetSetMethod(true));
		var b = new Button();
		Assert.False(b.HasContent);
		b.Content = "x";
		Assert.True(b.HasContent);
		b.Content = null;
		Assert.False(b.HasContent);
	});

	[Fact]
	public void TextBox_Cut_acts_on_the_live_widget_selection() => Run(() =>
	{
		var tb = new TextBox { Text = "hello world" };
		((Gtk.Entry)tb.Widget).SelectRegion(6, 11);
		tb.Cut();
		Assert.Equal("hello ", tb.Text);
		Assert.Equal(string.Empty, tb.SelectedText);
	});

	[Fact]
	public void A_Text_binding_without_a_path_match_follows_the_selected_item() => Run(() =>
	{
		var list = new List<Division> { new() { Name = "Alpha" }, new() { Name = "Beta" } };
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = list };
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = list });
		Assert.Equal("Alpha", text.Text);
		lb.SelectedItem = list[1];
		Assert.Equal("Beta", text.Text);
	});

	[Fact]
	public void A_slash_path_resolves_against_the_current_item_of_a_child_collection() => Run(() =>
	{
		var company = new Company { Divisions = { new Division { Name = "North" }, new Division { Name = "South" } } };
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding("Divisions/Name") { Source = company });
		Assert.Equal("North", text.Text);
		CollectionViewSource.GetDefaultView(company.Divisions)!.MoveCurrentToNext();
		Assert.Equal("South", text.Text);
	});

	[Fact]
	public void A_lone_slash_binds_the_current_item_and_an_empty_path_keeps_the_collection() => Run(() =>
	{
		var list = new List<Division> { new() { Name = "Alpha" }, new() { Name = "Beta" } };
		var current = new ContentControl();
		current.SetBinding(ContentControl.ContentProperty, new Binding("/") { Source = list });
		Assert.Same(list[0], current.Content);
		var whole = new ContentControl();
		whole.SetBinding(ContentControl.ContentProperty, new Binding { Source = list });
		Assert.Same(list, whole.Content);
	});
}
