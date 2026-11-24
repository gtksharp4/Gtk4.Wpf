using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace System.Windows.Controls;

/// <summary>
/// Hosts 3D content. Backed by a <see cref="Gtk.DrawingArea"/> painted by a managed software rasterizer on Cairo
/// (see <c>SceneRenderer</c>); any change to the camera or to the content queues one redraw per frame.
/// </summary>
[ContentProperty("Children")]
public class Viewport3D : FrameworkElement, IDisposable, IVisual3DOwner
{
	/// <summary>Identifies <see cref="Camera"/>.</summary>
	public static readonly DependencyProperty CameraProperty = DependencyProperty.Register(
		nameof(Camera), typeof(Camera), typeof(Viewport3D),
		new PropertyMetadata(DefaultCamera(), (d, e) => ((Viewport3D)d).OnCameraChanged(e)));

	static Viewport3D()
	{
		ClipToBoundsProperty.OverrideMetadata(typeof(Viewport3D), new PropertyMetadata(true));
	}

	static Camera DefaultCamera()
	{
		var camera = new PerspectiveCamera();
		camera.Freeze();
		return camera;
	}

	readonly System.Windows.Media.FreezableChildWatcher _cameraWatcher;
	Gtk.DrawingArea? _area;
	bool _disposed;

	/// <summary>Creates an empty viewport.</summary>
	public Viewport3D()
	{
		Children = new Visual3DCollection(this);
		_cameraWatcher = new System.Windows.Media.FreezableChildWatcher(Invalidate3D);
	}

	/// <summary>The camera that views the scene.</summary>
	public Camera Camera
	{
		get => (Camera)GetValue(CameraProperty)!;
		set => SetValue(CameraProperty, value);
	}

	/// <summary>The 3D content.</summary>
	public Visual3DCollection Children { get; }

	void OnCameraChanged(DependencyPropertyChangedEventArgs e)
	{
		_cameraWatcher.Set(e.Property, e.NewValue);
		Invalidate3D();
	}

	/// <summary>Queues a redraw; GTK coalesces repeated requests into one per frame.</summary>
	internal void Invalidate3D()
	{
		InvalidateCount++;
		_area?.QueueDraw();
	}

	/// <summary>Number of redraw requests received (test hook).</summary>
	internal int InvalidateCount { get; private set; }

	void IVisual3DOwner.Invalidate3D() => Invalidate3D();

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_area = new Gtk.DrawingArea();
		_area.DrawFunc = CairoDrawFunc.Wrap(OnDraw);
		return _area;
	}

	void OnDraw(Cairo.Context cr, int width, int height)
	{
		if (width > 0 && height > 0) PaintTo(cr, width, height);
	}

	/// <summary>Paints the scene exactly as the drawing area does.</summary>
	internal void PaintTo(Cairo.Context cr, int width, int height) =>
		SceneRenderer.Render(cr, (Camera?)GetValue(CameraProperty), Children, width, height);

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the draw callback on a deterministic dispose only.</summary>
	protected virtual void Dispose(bool disposing)
	{
		if (_disposed) return;
		if (disposing && _area != null)
		{
			_area.DrawFunc = null!;
			_area = null;
		}
		_disposed = true;
	}
}
