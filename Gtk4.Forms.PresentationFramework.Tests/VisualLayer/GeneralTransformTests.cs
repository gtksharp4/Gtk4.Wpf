using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class GeneralTransformTests
{
	[Fact]
	public void Transform_derives_from_GeneralTransform()
	{
		GeneralTransform t = new TranslateTransform(3, 4);
		Assert.Equal(new Point(4, 6), t.Transform(new Point(1, 2)));
		Assert.True(t.TryTransform(new Point(0, 0), out var p));
		Assert.Equal(new Point(3, 4), p);
	}

	[Fact]
	public void Inverse_round_trips_and_singular_has_none()
	{
		var t = new ScaleTransform(2, 4);
		var inv = t.Inverse!;
		Assert.Equal(new Point(1, 1), inv.Transform(t.Transform(new Point(1, 1))));
		Assert.Null(new MatrixTransform(new Matrix(0, 0, 0, 0, 0, 0)).Inverse);
	}

	[Fact]
	public void TransformBounds_is_the_bounding_box()
	{
		var b = new RotateTransform(90).TransformBounds(new Rect(0, 0, 10, 20));
		Assert.Equal(20, b.Width, 6);
		Assert.Equal(10, b.Height, 6);
		Assert.True(new TranslateTransform(1, 1).TransformBounds(Rect.Empty).IsEmpty);
	}

	[Fact]
	public void Group_applies_children_in_order_and_inverts()
	{
		var g = new GeneralTransformGroup();
		g.Children.Add(new TranslateTransform(10, 0));
		g.Children.Add(new ScaleTransform(2, 2));
		Assert.Equal(new Point(22, 2), g.Transform(new Point(1, 1)));
		Assert.Equal(new Point(1, 1), g.Inverse!.Transform(new Point(22, 2)));
		Assert.Equal(new Rect(20, 0, 2, 2), g.TransformBounds(new Rect(0, 0, 1, 1)));
	}

	[Fact]
	public void Group_clone_copies_children()
	{
		var g = new GeneralTransformGroup();
		g.Children.Add(new TranslateTransform(1, 2));
		var c = (GeneralTransformGroup)g.Clone();
		Assert.Single(c.Children);
		Assert.NotSame(g.Children[0], c.Children[0]);
	}

	[Fact]
	public void Transform_throws_when_the_point_cannot_be_mapped()
	{
		var g = new GeneralTransformGroup();
		g.Children.Add(new FailingTransform());
		Assert.Throws<InvalidOperationException>(() => g.Transform(new Point(0, 0)));
	}

	sealed class FailingTransform : GeneralTransform
	{
		public override GeneralTransform? Inverse => null;
		public override bool TryTransform(Point inPoint, out Point result) { result = inPoint; return false; }
		public override Rect TransformBounds(Rect rect) => rect;
		protected override Freezable CreateInstanceCore() => new FailingTransform();
	}
}
