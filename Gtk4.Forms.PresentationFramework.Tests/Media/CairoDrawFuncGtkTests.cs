using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Gtk4.Forms.PresentationFramework.Tests.VisualLayer;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>
/// Real-GTK: every DrawingArea paint disposes the Cairo.Context GtkSharp hands the draw callback, so repainting
/// never leaves a wrapper for the finalizer thread (which prints "Cairo.Context is leaking").
/// </summary>
public class CairoDrawFuncGtkTests : WpfGtkTestBase
{
	const int Frames = 25;

	static Viewport3D Scene()
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		mesh.Positions.Add(new Point3D(0, 1, 0));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		var viewport = new Viewport3D
		{
			Width = 80, Height = 80,
			Camera = new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 90),
		};
		viewport.Children.Add(new ModelVisual3D
		{
			Content = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Red)),
		});
		return viewport;
	}

	/// <summary>Shows <paramref name="element"/>, repaints it <see cref="Frames"/> times and returns the contexts.</summary>
	static List<Cairo.Context> Repaint(FrameworkElement element, Action invalidate)
	{
		var seen = new List<Cairo.Context>();
		CairoDrawFunc.Disposed = seen.Add;
		var win = new WpfWindow { Width = 200, Height = 200, Content = element };
		try
		{
			using var gtk = new GtkDispatcherScope();
			win.Show();
			GtkPump.Until(() => seen.Count > 0);
			for (int i = 0; i < Frames; i++)
			{
				invalidate();
				GtkPump.For(20);
			}
		}
		finally
		{
			CairoDrawFunc.Disposed = null;
			win.Close();
		}
		return seen;
	}

	static void AssertAllDisposed(List<Cairo.Context> seen)
	{
		Assert.True(seen.Count > 1, "the drawing area never painted");
		foreach (var cr in seen)
		{
			Assert.Equal(IntPtr.Zero, cr.Handle);
			Assert.Throws<ObjectDisposedException>(() => cr.Save());
		}
	}

	static string LeakOutputAfterCollect(FrameworkElement element, Action invalidate)
	{
		var previous = Console.Error;
		var captured = new StringWriter();
		try
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Console.SetError(captured);
			var seen = Repaint(element, invalidate);
			AssertAllDisposed(seen);
			seen.Clear();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
		finally { Console.SetError(previous); }
		return captured.ToString();
	}

	[Fact]
	public void An_OnRender_element_disposes_the_context_of_every_paint() => Run(() =>
	{
		var element = new RenderingElement { Width = 50, Height = 50 };
		AssertAllDisposed(Repaint(element, element.InvalidateRenderLayer));
	});

	[Fact]
	public void A_shape_disposes_the_context_of_every_paint() => Run(() =>
	{
		var shape = new Rectangle { Width = 50, Height = 50, Fill = Brushes.Red };
		int n = 0;
		AssertAllDisposed(Repaint(shape, () => shape.Fill = ++n % 2 == 0 ? Brushes.Red : Brushes.Blue));
	});

	[Fact]
	public void A_Viewport3D_disposes_the_context_of_every_paint() => Run(() =>
	{
		var viewport = Scene();
		AssertAllDisposed(Repaint(viewport, viewport.Invalidate3D));
	});

	[Fact]
	public void Repainting_an_OnRender_element_prints_no_leak_warning() => Run(() =>
	{
		var element = new RenderingElement { Width = 50, Height = 50 };
		Assert.DoesNotContain("is leaking", LeakOutputAfterCollect(element, element.InvalidateRenderLayer));
	});

	[Fact]
	public void Repainting_a_Viewport3D_prints_no_leak_warning() => Run(() =>
	{
		var viewport = Scene();
		Assert.DoesNotContain("is leaking", LeakOutputAfterCollect(viewport, viewport.Invalidate3D));
	});

	[Fact]
	public void A_painter_that_throws_still_disposes_the_context() => Run(() =>
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 4, 4);
		var cr = new Cairo.Context(surface);
		var draw = CairoDrawFunc.Wrap((c, w, h) => throw new InvalidOperationException("boom"));
		Assert.Throws<InvalidOperationException>(() => draw(null!, cr, 4, 4));
		Assert.Equal(IntPtr.Zero, cr.Handle);
	});
}
