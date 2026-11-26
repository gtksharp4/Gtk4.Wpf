using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>Builders and pixel readers for the textured-material rendering tests.</summary>
static class TexturedSceneHelpers
{
	internal const int Size = 200;

	/// <summary>A 64x64 PNG whose top half is red and bottom half blue.</summary>
	internal static string TwoColourPng(string name = "two-colour")
	{
		string dir = Path.Combine(AppContext.BaseDirectory, "lane-h-textures");
		Directory.CreateDirectory(dir);
		string path = Path.Combine(dir, name + ".png");
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 64, 64);
		using (var cr = new Cairo.Context(surface))
		{
			cr.SetSourceRGB(1, 0, 0);
			cr.Rectangle(0, 0, 64, 32);
			cr.Fill();
			cr.SetSourceRGB(0, 0, 1);
			cr.Rectangle(0, 32, 64, 32);
			cr.Fill();
		}
		surface.WriteToPng(path);
		return path;
	}

	/// <summary>A 64x64 PNG whose left half is fully transparent and right half opaque red.</summary>
	internal static string HalfTransparentPng()
	{
		string dir = Path.Combine(AppContext.BaseDirectory, "lane-h-textures");
		Directory.CreateDirectory(dir);
		string path = Path.Combine(dir, "half-transparent.png");
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 64, 64);
		using (var cr = new Cairo.Context(surface))
		{
			cr.SetSourceRGB(1, 0, 0);
			cr.Rectangle(32, 0, 32, 64);
			cr.Fill();
		}
		surface.WriteToPng(path);
		return path;
	}

	internal static ImageBrush BrushOf(string path) => new ImageBrush(new BitmapImage(new Uri(path)));

	/// <summary>The sample's plane: -1..1 at z=0, v=0 at the top.</summary>
	internal static MeshGeometry3D Plane() => new MeshGeometry3D
	{
		Positions = Point3DCollection.Parse("-1 -1 0  1 -1 0  -1 1 0  1 1 0"),
		TextureCoordinates = new System.Windows.Shapes.PointCollection
		{
			new System.Windows.Point(0, 1), new System.Windows.Point(1, 1),
			new System.Windows.Point(0, 0), new System.Windows.Point(1, 0),
		},
		TriangleIndices = Int32Collection.Parse("0 1 2  1 3 2"),
	};

	internal static Viewport3D Viewport(Model3D model, Point3D? eye = null, Vector3D? look = null)
	{
		var viewport = new Viewport3D
		{
			Camera = new PerspectiveCamera(eye ?? new Point3D(0, 0, 5), look ?? new Vector3D(0, 0, -1),
				new Vector3D(0, 1, 0), 60),
		};
		viewport.Children.Add(new ModelVisual3D { Content = model });
		return viewport;
	}

	internal static Model3DGroup Lit(GeometryModel3D geometry, Color? ambient = null)
	{
		var group = new Model3DGroup();
		group.Children.Add(new AmbientLight(ambient ?? Colors.White));
		group.Children.Add(geometry);
		return group;
	}

	/// <summary>Paints over a solid green backdrop so unpainted pixels stay recognisable.</summary>
	internal static Cairo.ImageSurface Paint(Viewport3D viewport)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		cr.SetSourceRGB(0, 1, 0);
		cr.Paint();
		// GTK hands the draw callback a context whose source is black; keep that default.
		cr.SetSourceRGB(0, 0, 0);
		viewport.PaintTo(cr, Size, Size);
		return surface;
	}

	internal static (byte R, byte G, byte B, byte A) PixelAt(Cairo.ImageSurface surface, int x, int y)
	{
		surface.Flush();
		byte[] data = surface.Data;
		int i = y * surface.Stride + x * 4;
		return (data[i + 2], data[i + 1], data[i], data[i + 3]);
	}

	internal static bool IsBackdrop((byte R, byte G, byte B, byte A) p) => p.R == 0 && p.G == 255 && p.B == 0;
}
