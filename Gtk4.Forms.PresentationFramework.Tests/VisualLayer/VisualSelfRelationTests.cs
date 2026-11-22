using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

/// <summary>A visual is its own ancestor and descendant, as in WPF.</summary>
public class VisualSelfRelationTests
{
	class NotAVisual : DependencyObject { }

	[Fact]
	public void A_visual_is_its_own_ancestor_and_descendant()
	{
		var v = new ContainerVisual();
		Assert.True(v.IsAncestorOf(v));
		Assert.True(v.IsDescendantOf(v));
	}

	[Fact]
	public void Self_relations_hold_inside_a_tree_and_unrelated_visuals_stay_unrelated()
	{
		var root = new ContainerVisual();
		var child = new ContainerVisual();
		root.Children.Add(child);
		Assert.True(root.IsAncestorOf(child));
		Assert.False(child.IsAncestorOf(root));
		Assert.True(child.IsDescendantOf(root));
		Assert.False(root.IsDescendantOf(child));
		Assert.False(root.IsAncestorOf(new ContainerVisual()));
	}

	[Fact]
	public void TransformToAncestor_and_Descendant_of_self_are_identity()
	{
		var v = new ContainerVisual { Offset = new Vector(5, 6) };
		Assert.Equal(new Point(1, 2), v.TransformToAncestor(v).Transform(new Point(1, 2)));
		Assert.Equal(new Point(1, 2), v.TransformToDescendant(v)!.Transform(new Point(1, 2)));
	}

	[Fact]
	public void Unrelated_visuals_still_throw_from_the_transform_methods()
	{
		var a = new ContainerVisual();
		var b = new ContainerVisual();
		Assert.Throws<InvalidOperationException>(() => a.TransformToAncestor(b));
		Assert.Throws<InvalidOperationException>(() => a.TransformToDescendant(b));
	}

	[Fact]
	public void Null_arguments_throw_ArgumentNullException()
	{
		var v = new ContainerVisual();
		Assert.Throws<ArgumentNullException>(() => v.IsAncestorOf(null!));
		Assert.Throws<ArgumentNullException>(() => v.IsDescendantOf(null!));
		Assert.Throws<ArgumentNullException>(() => v.TransformToAncestor(null!));
		Assert.Throws<ArgumentNullException>(() => v.TransformToDescendant(null!));
	}

	[Fact]
	public void A_non_visual_argument_throws_ArgumentException()
	{
		var v = new ContainerVisual();
		Assert.Throws<ArgumentException>(() => v.IsAncestorOf(new NotAVisual()));
		Assert.Throws<ArgumentException>(() => v.IsDescendantOf(new NotAVisual()));
	}
}
