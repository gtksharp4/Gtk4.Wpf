using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Pure tests for TextBlock.TextEffects and object-typed literal conversion.</summary>
public class TextBlockTextEffectsTests
{
	[Fact]
	public void TextEffectsProperty_is_the_TextElement_property()
		=> Assert.Same(TextElement.TextEffectsProperty, TextBlock.TextEffectsProperty);

	[Fact]
	public void TextEffectsProperty_is_owned_by_TextBlock_and_affects_render()
	{
		Assert.Equal(typeof(TextEffectCollection), TextBlock.TextEffectsProperty.PropertyType);
		var meta = (FrameworkPropertyMetadata)TextBlock.TextEffectsProperty.GetMetadata(typeof(TextBlock));
		Assert.True(meta.AffectsRender);
	}

	[Fact]
	public void Reading_TextEffects_does_not_create_a_local_value()
	{
		var tb = new TextBlock();

		_ = tb.TextEffects;

		Assert.Same(DependencyProperty.UnsetValue, tb.ReadLocalValue(TextBlock.TextEffectsProperty));
	}

	[Fact]
	public void A_style_setter_wins_after_a_read()
	{
		var tb = new TextBlock();
		_ = tb.TextEffects;
		var effect = new TextEffect { PositionCount = 3 };
		var style = new Style(typeof(TextBlock));
		style.Setters.Add(new Setter(TextBlock.TextEffectsProperty, new TextEffectCollection { effect }));

		tb.Style = style;

		Assert.Same(effect, Assert.Single(tb.TextEffects!));
	}

	[Fact]
	public void Effects_added_to_a_created_collection_are_kept()
	{
		var tb = new TextBlock { TextEffects = new TextEffectCollection() };
		var effect = new TextEffect { PositionStart = 1, PositionCount = 2 };

		tb.TextEffects.Add(effect);

		Assert.Same(effect, Assert.Single(tb.TextEffects));
	}

	[Fact]
	public void TextEffects_can_be_replaced()
	{
		var tb = new TextBlock();
		var coll = new TextEffectCollection { new TextEffect() };

		tb.TextEffects = coll;

		Assert.Same(coll, tb.TextEffects);
	}

	[Fact]
	public void Object_typed_dependency_property_literal_stays_a_string()
	{
		Assert.Equal("1", XamlValueConverter.Convert(typeof(FrameworkElement), FrameworkElement.TagProperty, "1"));
		Assert.Equal("true", XamlValueConverter.Convert(typeof(Button), ContentControl.ContentProperty, "true"));
		Assert.Equal("2", XamlValueConverter.Convert(typeof(Button), Button.CommandParameterProperty, "2"));
	}
}
