using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

/// <summary>BitmapEffectCollection is a freezable list (an Animatable), as in WPF, not an ObservableCollection.</summary>
public class BitmapEffectCollectionTests
{
	[Fact]
	public void It_is_an_Animatable_list_of_BitmapEffect()
	{
		var c = new BitmapEffectCollection();
		Assert.IsAssignableFrom<Animatable>(c);
		Assert.IsAssignableFrom<IList<BitmapEffect>>(c);
		Assert.IsAssignableFrom<IList>(c);
		var blur = new BlurBitmapEffect();
		c.Add(blur);
		Assert.Equal(1, c.Count);
		Assert.Same(blur, c[0]);
		Assert.True(c.Contains(blur));
		c.RemoveAt(0);
		Assert.Empty(c);
	}

	[Fact]
	public void Freezing_makes_it_read_only_and_clone_is_deep()
	{
		var c = new BitmapEffectCollection { new BlurBitmapEffect { Radius = 3 } };
		var clone = c.Clone();
		Assert.NotSame(c[0], clone[0]);
		Assert.Equal(3.0, ((BlurBitmapEffect)clone[0]).Radius);
		c.Freeze();
		Assert.Throws<InvalidOperationException>(() => c.Add(new BlurBitmapEffect()));
		Assert.True(c[0].IsFrozen);
	}

	[Fact]
	public void Changing_the_children_of_a_group_raises_Changed()
	{
		var group = new BitmapEffectGroup();
		int changes = 0;
		group.Changed += (s, e) => changes++;
		group.Children.Add(new BlurBitmapEffect());
		Assert.True(changes > 0);
		int before = changes;
		group.Children[0] = new EmbossBitmapEffect();
		Assert.True(changes > before);
	}
}
