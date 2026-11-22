using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class HitTestTests : WpfGtkTestBase
{
	[Fact]
	public void Point_inside_drawing_bounds_hits_and_outside_misses() => Run(() =>
	{
		var v = new RectVisual(new Rect(10, 10, 20, 20));
		var hit = Assert.IsType<PointHitTestResult>(VisualTreeHelper.HitTest(v, new Point(15, 15)));
		Assert.Same(v, hit.VisualHit);
		Assert.Equal(new Point(15, 15), hit.PointHit);
		Assert.Null(VisualTreeHelper.HitTest(v, new Point(5, 5)));
		Assert.Null(VisualTreeHelper.HitTest(v, new Point(30, 30)));
	});

	[Fact]
	public void Topmost_child_wins_and_child_offset_is_applied() => Run(() =>
	{
		var root = new ContainerVisual();
		var below = new RectVisual(new Rect(0, 0, 50, 50));
		var above = new RectVisual(new Rect(0, 0, 10, 10), new Vector(20, 20));
		root.Children.Add(below);
		root.Children.Add(above);
		Assert.Same(above, VisualTreeHelper.HitTest(root, new Point(25, 25))!.VisualHit);
		Assert.Same(below, VisualTreeHelper.HitTest(root, new Point(5, 5))!.VisualHit);
		var hit = (PointHitTestResult)VisualTreeHelper.HitTest(root, new Point(25, 25))!;
		Assert.Equal(new Point(5, 5), hit.PointHit);
	});

	[Fact]
	public void Callback_Continue_reports_every_hit_in_z_order_and_Stop_ends() => Run(() =>
	{
		var root = new ContainerVisual();
		var a = new RectVisual(new Rect(0, 0, 50, 50));
		var b = new RectVisual(new Rect(0, 0, 50, 50));
		root.Children.Add(a);
		root.Children.Add(b);
		var seen = new List<Visual>();
		VisualTreeHelper.HitTest(root, null, r => { seen.Add(r.VisualHit); return HitTestResultBehavior.Continue; },
			new PointHitTestParameters(new Point(1, 1)));
		Assert.Equal(new Visual[] { b, a }, seen);
		seen.Clear();
		root.HitTest(null, r => { seen.Add(r.VisualHit); return HitTestResultBehavior.Stop; },
			new PointHitTestParameters(new Point(1, 1)));
		Assert.Single(seen);
	});

	[Fact]
	public void Filter_behaviors_skip_self_children_or_everything() => Run(() =>
	{
		var root = new ContainerVisual();
		var child = new RectVisual(new Rect(0, 0, 10, 10));
		root.Children.Add(child);
		int Hits(HitTestFilterBehavior onChild)
		{
			int n = 0;
			VisualTreeHelper.HitTest(root, d => d == child ? onChild : HitTestFilterBehavior.Continue,
				r => { n++; return HitTestResultBehavior.Continue; }, new PointHitTestParameters(new Point(1, 1)));
			return n;
		}
		Assert.Equal(1, Hits(HitTestFilterBehavior.Continue));
		Assert.Equal(0, Hits(HitTestFilterBehavior.ContinueSkipSelf));
		Assert.Equal(0, Hits(HitTestFilterBehavior.ContinueSkipSelfAndChildren));
		Assert.Equal(1, Hits(HitTestFilterBehavior.ContinueSkipChildren));
		Assert.Equal(0, Hits(HitTestFilterBehavior.Stop));
	});

	[Fact]
	public void Geometry_parameters_report_the_intersection_detail() => Run(() =>
	{
		var v = new RectVisual(new Rect(0, 0, 20, 20));
		HitTestResult? result = null;
		VisualTreeHelper.HitTest(v, null, r => { result = r; return HitTestResultBehavior.Stop; },
			new GeometryHitTestParameters(new RectangleGeometry(new Rect(5, 5, 5, 5))));
		Assert.Equal(IntersectionDetail.FullyInside, Assert.IsType<GeometryHitTestResult>(result).IntersectionDetail);
		result = null;
		VisualTreeHelper.HitTest(v, null, r => { result = r; return HitTestResultBehavior.Stop; },
			new GeometryHitTestParameters(new RectangleGeometry(new Rect(50, 50, 5, 5))));
		Assert.Null(result);
	});

	[Fact]
	public void HitTestCore_override_replaces_the_default_and_base_still_works() => Run(() =>
	{
		var v = new OverridingVisual();
		using (var dc = v.RenderOpen()) dc.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 10, 10));
		Assert.Null(VisualTreeHelper.HitTest(v, new Point(1, 1)));
		v.Allow = true;
		Assert.NotNull(VisualTreeHelper.HitTest(v, new Point(1, 1)));
	});

	[Fact]
	public void Invisible_or_not_hit_testable_elements_are_pruned() => Run(() =>
	{
		var host = new VisualHost();
		var child = new VisualHost { Width = 10, Height = 10 };
		host.Visuals.Add(child);
		host.AddV(child);
		child.RenderSize = new Size(10, 10);
		int n = 0;
		HitTestResultCallback count = r => { n++; return HitTestResultBehavior.Continue; };
		VisualTreeHelper.HitTest(host, null, count, new PointHitTestParameters(new Point(1, 1)));
		Assert.Equal(1, n);
		child.IsHitTestVisible = false;
		n = 0;
		VisualTreeHelper.HitTest(host, null, count, new PointHitTestParameters(new Point(1, 1)));
		Assert.Equal(0, n);
		child.IsHitTestVisible = true;
		child.Visibility = Visibility.Hidden;
		VisualTreeHelper.HitTest(host, null, count, new PointHitTestParameters(new Point(1, 1)));
		Assert.Equal(0, n);
	});

	[Fact]
	public void Null_arguments_are_rejected() => Run(() =>
	{
		var v = new DrawingVisual();
		Assert.Throws<System.ArgumentNullException>(() => VisualTreeHelper.HitTest(null!, new Point()));
		Assert.Throws<System.ArgumentNullException>(() =>
			VisualTreeHelper.HitTest(v, null, null!, new PointHitTestParameters(new Point())));
		Assert.Throws<System.ArgumentNullException>(() =>
			VisualTreeHelper.HitTest(v, null, r => HitTestResultBehavior.Stop, null!));
	});

	class OverridingVisual : DrawingVisual
	{
		public bool Allow;
		protected override HitTestResult? HitTestCore(PointHitTestParameters p) => Allow ? base.HitTestCore(p) : null;
		protected override GeometryHitTestResult? HitTestCore(GeometryHitTestParameters p) => null;
	}
}
