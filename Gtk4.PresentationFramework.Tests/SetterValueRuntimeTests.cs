using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class SetterValueRuntimeTests
{
	static readonly DependencyProperty Tag = SetterTestElement.TagProperty;

	[Fact]
	public void Ctor_property_value()
	{
		var s = new Setter(Tag, "x");
		Assert.Same(Tag, s.Property);
		Assert.Equal("x", s.Value);
		Assert.Null(s.TargetName);
	}

	[Fact]
	public void Ctor_with_target_name()
	{
		var s = new Setter(Tag, "x", "part");
		Assert.Equal("part", s.TargetName);
	}

	[Fact]
	public void Null_property_throws()
	{
		Assert.Throws<ArgumentNullException>(() => new Setter(null!, "x"));
		Assert.Throws<ArgumentNullException>(() => new Setter().Property = null!);
	}

	[Fact]
	public void PropertyName_resolves_owner_qualified_forms()
	{
		foreach (var name in new[] { "SetterTestTag", "SetterTestElement.SetterTestTag", "(SetterTestElement.SetterTestTag)" })
		{
			var el = new SetterTestElement();
			new Setter { PropertyName = name, Value = "v" }.ApplyToStyleLayer(el);
			Assert.Equal("v", el.Tag2);
		}
	}

	[Fact]
	public void PropertyName_resolves_base_owner_from_assembly_types()
	{
		var el = new SetterTestElement();
		new Setter { PropertyName = "UIElement.IsEnabled", Value = false }.ApplyToStyleLayer(el);
		Assert.False((bool)el.GetValue(UIElement.IsEnabledProperty)!);
	}

	[Fact]
	public void UnsetValue_throws_ArgumentException_at_assignment()
	{
		var ex = Assert.Throws<ArgumentException>(() => new Setter { Value = DependencyProperty.UnsetValue });
		Assert.Contains("UnsetValue", ex.Message);
		Assert.Throws<ArgumentException>(() => new Setter(Tag, DependencyProperty.UnsetValue));
	}

	[Fact]
	public void Null_property_reports_ParamName_property()
	{
		Assert.Equal("property", Assert.Throws<ArgumentNullException>(() => new Setter(null!, "x")).ParamName);
		Assert.Equal("property", Assert.Throws<ArgumentNullException>(() => new Setter().Property = null!).ParamName);
	}

	[Fact]
	public void Read_only_property_throws_ArgumentException()
	{
		var ro = UIElement.IsMouseOverProperty;
		Assert.Throws<ArgumentException>(() => new Setter(ro, true));
		Assert.Throws<ArgumentException>(() => new Setter(ro, true, "part"));
		Assert.Throws<ArgumentException>(() => new Setter().Property = ro);
	}

	[Fact]
	public void Object_value_is_applied_as_is()
	{
		var el = new SetterTestElement();
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, "plain"));
		el.Style = style;
		Assert.Equal("plain", el.Tag2);
	}

	[Fact]
	public void StaticResource_resolves_at_apply()
	{
		var el = new SetterTestElement();
		el.Resources["k"] = "first";
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, new StaticResourceExtension("k")));
		el.Style = style;
		el.Resources["k"] = "second";
		Assert.Equal("first", el.Tag2);
	}

	[Fact]
	public void Style_DynamicResource_tracks_replacement_and_clears_on_unapply()
	{
		var el = new SetterTestElement();
		el.Resources["k"] = "one";
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, new DynamicResourceExtension("k")));
		el.Style = style;
		Assert.Equal("one", el.Tag2);

		el.Resources["k"] = "two";
		Assert.Equal("two", el.Tag2);

		el.Style = null;
		Assert.Null(el.Tag2);
		el.Resources["k"] = "three";
		Assert.Null(el.Tag2);
	}

	[Fact]
	public void Local_value_wins_over_style_DynamicResource()
	{
		var el = new SetterTestElement { Tag2 = "local" };
		el.Resources["k"] = "res";
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, new DynamicResourceExtension("k")));
		el.Style = style;
		Assert.Equal("local", el.Tag2);
	}

	[Fact]
	public void Style_Binding_binds_while_applied_and_stops_on_unapply()
	{
		var src = new SetterTestSource { Name = "bound" };
		var el = new SetterTestElement { DataContext = src };
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, new Binding("Name")));
		el.Style = style;
		Assert.Equal("bound", el.Tag2);
		Assert.NotNull(BindingOperations.GetBindingExpression(el, Tag));

		el.Style = null;
		Assert.Null(el.Tag2);
		Assert.Null(BindingOperations.GetBindingExpression(el, Tag));
	}

	[Fact]
	public void Local_value_wins_over_style_Binding()
	{
		var el = new SetterTestElement { DataContext = new SetterTestSource { Name = "bound" }, Tag2 = "local" };
		var style = new Style(typeof(SetterTestElement));
		style.Setters.Add(new Setter(Tag, new Binding("Name")));
		el.Style = style;
		Assert.Equal("local", el.Tag2);
		Assert.Null(BindingOperations.GetBindingExpression(el, Tag));
	}

	[Fact]
	public void Trigger_DynamicResource_applies_tracks_and_reverts()
	{
		var el = new SetterTestElement();
		el.Resources["k"] = "one";
		var style = new Style(typeof(SetterTestElement));
		var t = new Trigger { Property = SetterTestElement.FlagProperty, Value = true };
		t.Setters.Add(new Setter(Tag, new DynamicResourceExtension("k")));
		style.Triggers.Add(t);
		el.Style = style;
		Assert.Null(el.Tag2);

		el.Flag = true;
		Assert.Equal("one", el.Tag2);
		el.Resources["k"] = "two";
		Assert.Equal("two", el.Tag2);

		el.Flag = false;
		Assert.Null(el.Tag2);
		el.Resources["k"] = "three";
		Assert.Null(el.Tag2);
	}

	[Fact]
	public void Trigger_Binding_binds_when_active_and_clears_when_inactive()
	{
		var el = new SetterTestElement { DataContext = new SetterTestSource { Name = "n" } };
		var style = new Style(typeof(SetterTestElement));
		var t = new Trigger { Property = SetterTestElement.FlagProperty, Value = true };
		t.Setters.Add(new Setter(Tag, new Binding("Name")));
		style.Triggers.Add(t);
		el.Style = style;

		el.Flag = true;
		Assert.Equal("n", el.Tag2);
		el.Flag = false;
		Assert.Null(el.Tag2);
		Assert.Null(BindingOperations.GetBindingExpression(el, Tag));
	}
}
