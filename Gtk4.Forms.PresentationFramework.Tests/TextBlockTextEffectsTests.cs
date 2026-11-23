using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

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
	public void TextEffects_is_never_null_and_is_stable()
	{
		var tb = new TextBlock();

		Assert.NotNull(tb.TextEffects);
		Assert.Same(tb.TextEffects, tb.TextEffects);
		Assert.Same(tb.TextEffects, tb.GetValue(TextBlock.TextEffectsProperty));
	}

	[Fact]
	public void Effects_added_to_the_implicit_collection_are_kept()
	{
		var tb = new TextBlock();
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
