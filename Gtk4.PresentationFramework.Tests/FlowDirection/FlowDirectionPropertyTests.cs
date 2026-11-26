using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

public class FlowDirectionPropertyTests
{
	[Fact]
	public void Enum_has_WPF_values()
	{
		Assert.Equal(0, (int)FlowDirection.LeftToRight);
		Assert.Equal(1, (int)FlowDirection.RightToLeft);
	}

	[Fact]
	public void Default_is_LeftToRight_and_metadata_matches_WPF()
	{
		var meta = (FrameworkPropertyMetadata)FrameworkElement.FlowDirectionProperty.GetMetadata(typeof(Button));
		Assert.Equal(FlowDirection.LeftToRight, meta.DefaultValue);
		Assert.True(meta.Inherits);
		Assert.True(meta.AffectsParentArrange);
		Assert.False(meta.AffectsArrange);
		Assert.Equal(FlowDirection.LeftToRight, new Button().FlowDirection);
	}

	[Fact]
	public void Attached_accessors_round_trip_on_any_DependencyObject()
	{
		var b = new Button();
		FrameworkElement.SetFlowDirection(b, FlowDirection.RightToLeft);
		Assert.Equal(FlowDirection.RightToLeft, FrameworkElement.GetFlowDirection(b));
		Assert.Equal(FlowDirection.RightToLeft, b.FlowDirection);
		Assert.Throws<System.ArgumentNullException>(() => FrameworkElement.GetFlowDirection(null!));
		Assert.Throws<System.ArgumentNullException>(
			() => FrameworkElement.SetFlowDirection(null!, FlowDirection.LeftToRight));
	}

	[Fact]
	public void Invalid_value_is_rejected()
	{
		var b = new Button();
		Assert.Throws<System.ArgumentException>(() => b.FlowDirection = (FlowDirection)7);
	}

	[Fact]
	public void Value_is_inherited_through_the_element_tree()
	{
		var outer = new StackPanel();
		var inner = new StackPanel();
		var leaf = new Button();
		outer.Children.Add(inner);
		inner.Children.Add(leaf);
		outer.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(FlowDirection.RightToLeft, leaf.FlowDirection);
		inner.FlowDirection = FlowDirection.LeftToRight;
		Assert.Equal(FlowDirection.LeftToRight, leaf.FlowDirection);
		inner.ClearValue(FrameworkElement.FlowDirectionProperty);
		Assert.Equal(FlowDirection.RightToLeft, leaf.FlowDirection);
	}

	[Fact]
	public void Late_added_child_inherits()
	{
		var panel = new StackPanel { FlowDirection = FlowDirection.RightToLeft };
		var b = new Button();
		panel.Children.Add(b);
		Assert.Equal(FlowDirection.RightToLeft, b.FlowDirection);
	}

	[Fact]
	public void Language_defaults_to_en_us_and_inherits()
	{
		var meta = (FrameworkPropertyMetadata)FrameworkElement.LanguageProperty.GetMetadata(typeof(Button));
		Assert.True(meta.Inherits);
		Assert.True(meta.AffectsMeasure);
		var panel = new StackPanel();
		var b = new Button();
		panel.Children.Add(b);
		Assert.Equal("en-us", b.Language.IetfLanguageTag);
		panel.Language = XmlLanguage.GetLanguage("ar-SA");
		Assert.Equal("ar-sa", b.Language.IetfLanguageTag);
	}

	[Fact]
	public void XmlLanguage_is_cached_lowercase_and_validates()
	{
		Assert.Same(XmlLanguage.GetLanguage("AR-sa"), XmlLanguage.GetLanguage("ar-SA"));
		Assert.Equal("ar-sa", XmlLanguage.GetLanguage("AR-sa").ToString());
		Assert.Equal(string.Empty, XmlLanguage.Empty.IetfLanguageTag);
		Assert.Equal(new CultureInfo("ar-SA").Name, XmlLanguage.GetLanguage("ar-sa").GetSpecificCulture().Name);
		Assert.Throws<System.ArgumentNullException>(() => XmlLanguage.GetLanguage(null!));
	}

	[Fact]
	public void NumberSubstitution_matches_WPF_defaults()
	{
		Assert.Equal(new[] { "Text", "User", "Override" }, System.Enum.GetNames(typeof(NumberCultureSource)));
		Assert.Equal(
			new[] { "AsCulture", "Context", "European", "NativeNational", "Traditional" },
			System.Enum.GetNames(typeof(NumberSubstitutionMethod)));
		var tb = new TextBlock();
		Assert.Equal(NumberCultureSource.Text, NumberSubstitution.GetCultureSource(tb));
		Assert.Null(NumberSubstitution.GetCultureOverride(tb));
		Assert.Equal(NumberSubstitutionMethod.AsCulture, NumberSubstitution.GetSubstitution(tb));
		NumberSubstitution.SetCultureSource(tb, NumberCultureSource.User);
		NumberSubstitution.SetCultureOverride(tb, new CultureInfo("ar-SA"));
		NumberSubstitution.SetSubstitution(tb, NumberSubstitutionMethod.Traditional);
		Assert.Equal(NumberCultureSource.User, NumberSubstitution.GetCultureSource(tb));
		Assert.Equal("ar-SA", NumberSubstitution.GetCultureOverride(tb)!.Name);
		Assert.Equal(NumberSubstitutionMethod.Traditional, NumberSubstitution.GetSubstitution(tb));
	}
}
