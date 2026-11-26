using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextDecorationFreezableTests
{
	[Fact]
	public void Decoration_and_collection_are_Animatable()
	{
		Assert.True(typeof(Animatable).IsAssignableFrom(typeof(TextDecoration)));
		Assert.True(typeof(Animatable).IsAssignableFrom(typeof(TextDecorationCollection)));
	}

	[Fact]
	public void Frozen_collection_rejects_every_mutation()
	{
		var c = new TextDecorationCollection { new TextDecoration(TextDecorationLocation.Underline) };
		c.Freeze();
		Assert.True(c.IsFrozen);
		Assert.Throws<InvalidOperationException>(() => c.Add(new TextDecoration()));
		Assert.Throws<InvalidOperationException>(() => c.Clear());
		Assert.Throws<InvalidOperationException>(() => c.RemoveAt(0));
		Assert.Throws<InvalidOperationException>(() => c.Insert(0, new TextDecoration()));
		Assert.Throws<InvalidOperationException>(() => ((IList)c).Add(new TextDecoration()));
		Assert.Single(c);
	}

	[Fact]
	public void Freezing_the_collection_freezes_its_decorations()
	{
		var d = new TextDecoration(TextDecorationLocation.Strikethrough);
		var c = new TextDecorationCollection { d };
		c.Freeze();
		Assert.True(d.IsFrozen);
		Assert.Throws<InvalidOperationException>(() => d.Location = TextDecorationLocation.Baseline);
	}

	[Fact]
	public void Static_instances_are_frozen()
	{
		Assert.True(TextDecorations.Underline.IsFrozen);
		Assert.True(TextDecorations.Strikethrough.IsFrozen);
		Assert.True(TextDecorations.OverLine.IsFrozen);
		Assert.True(TextDecorations.Baseline.IsFrozen);
		Assert.Throws<InvalidOperationException>(() => TextDecorations.Underline.Add(new TextDecoration()));
	}

	[Fact]
	public void Clone_of_frozen_collection_is_modifiable_and_independent()
	{
		var clone = TextDecorations.Underline.Clone();
		Assert.False(clone.IsFrozen);
		clone.Add(new TextDecoration(TextDecorationLocation.OverLine));
		Assert.Equal(2, clone.Count);
		Assert.Single(TextDecorations.Underline);
		Assert.Equal(TextDecorationLocation.Underline, clone[0].Location);
	}

	[Fact]
	public void Changed_fires_on_mutation_and_Location_default_is_Underline()
	{
		var c = new TextDecorationCollection();
		int changes = 0;
		c.Changed += (s, e) => changes++;
		c.Add(new TextDecoration());
		Assert.Equal(1, changes);
		Assert.Equal(TextDecorationLocation.Underline, c[0].Location);
	}

	[Fact]
	public void Collection_behaves_as_a_list()
	{
		var a = new TextDecoration();
		var c = new TextDecorationCollection(new[] { a });
		Assert.Equal(0, c.IndexOf(a));
		Assert.True(c.Remove(a));
		Assert.Empty(c);
		Assert.Throws<ArgumentNullException>(() => c.Add(null!));
	}

	[Fact]
	public void IsMasterPage_attached_property_shape()
	{
		var e = new Border();
		Assert.False(DocumentViewerBase.GetIsMasterPage(e));
		DocumentViewerBase.SetIsMasterPage(e, true);
		Assert.True((bool)e.GetValue(DocumentViewerBase.IsMasterPageProperty)!);
		Assert.Equal(typeof(bool), DocumentViewerBase.IsMasterPageProperty.PropertyType);
		Assert.Equal("IsMasterPage", DocumentViewerBase.IsMasterPageProperty.Name);
		Assert.Throws<ArgumentNullException>(() => DocumentViewerBase.GetIsMasterPage(null!));
	}

	[Fact]
	public void IsMasterPage_is_inherited_down_the_tree()
	{
		var parent = new Border();
		var child = new Border();
		parent.Child = child;
		DocumentViewerBase.SetIsMasterPage(parent, true);
		Assert.True(DocumentViewerBase.GetIsMasterPage(child));
		var meta = (FrameworkPropertyMetadata)DocumentViewerBase.IsMasterPageProperty.GetMetadata(typeof(Border));
		Assert.True(meta.Inherits);
	}
}
