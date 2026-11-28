using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace System.Windows.Shapes;

// GTK size request: the WPF natural size is only a *natural* request (minimum 0), so the shape can be
// arranged smaller than its geometry and clipped like WPF, instead of forcing its parent to grow.
public abstract partial class Shape
{
	Size _natural;

	/// <summary>Installs the layout manager that reports minimum 0 and the natural size as natural.</summary>
	void InstallNaturalLayout(Gtk.DrawingArea area)
	{
		var layout = NaturalLayoutCallbacks.Create();
		if (layout == null) return;
		NaturalLayoutCallbacks.Owners.Add(area, this);
		area.LayoutManager = layout;
	}

	internal int NaturalRequest(Gtk.Orientation orientation)
	{
		bool horizontal = orientation == Gtk.Orientation.Horizontal;
		double natural = horizontal
			? ClampRequest(_natural.Width, Width, MinWidth, MaxWidth)
			: ClampRequest(_natural.Height, Height, MinHeight, MaxHeight);
		return ToRequest(Math.Min(natural, SlotLimit(horizontal)));
	}

	static double ClampRequest(double natural, double explicitSize, double min, double max) =>
		Math.Max(min, Math.Min(double.IsNaN(explicitSize) ? natural : explicitSize, max));

	// The slot cap is read when GTK measures, so a change to the slot asks GTK to re-measure.
	internal override void QueueSlotResize() => _area?.QueueResize();

	/// <summary>Refreshes the natural size (Stretch.None only) and asks GTK to re-measure.</summary>
	internal void SyncNaturalSize()
	{
		_natural = Stretch == Stretch.None ? GetNaturalSize() : new Size(0, 0);
		_area?.QueueResize();
	}

	static int ToRequest(double value) =>
		double.IsNaN(value) || value <= 0 ? 0 : (int)Math.Min(Math.Ceiling(value - 1e-6), 100000);
}

/// <summary>
/// Process-lifetime callbacks for <see cref="Gtk.CustomLayout"/>. GtkSharp's own constructor releases its native
/// delegate wrappers after the first call ("persist until called"), so a later call crashes with "callback on a
/// garbage collected delegate". The wrappers are therefore created here and rooted in static fields, and the layout
/// is created through GtkSharp's own native binding. Falls back to null (no layout manager) if that shape changes.
/// </summary>
static class NaturalLayoutCallbacks
{
	internal static readonly ConditionalWeakTable<Gtk.Widget, Shape> Owners = new();

	static readonly object[] _roots = new object[3];
	static readonly Delegate? _create;
	static readonly object[]? _natives;

	static NaturalLayoutCallbacks()
	{
		try
		{
			var asm = typeof(Gtk.CustomLayout).Assembly;
			const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
				| BindingFlags.NonPublic;
			_create = (Delegate?)typeof(Gtk.CustomLayout).GetField("gtk_custom_layout_new", all)?.GetValue(null);
			Gtk.CustomRequestModeFunc mode = _ => Gtk.SizeRequestMode.ConstantSize;
			Gtk.CustomMeasureFunc measure = Measure;
			Gtk.CustomAllocateFunc allocate = (_, _, _, _) => { };
			var managed = new Delegate[] { mode, measure, allocate };
			var names = new[] { "CustomRequestModeFuncWrapper", "CustomMeasureFuncWrapper", "CustomAllocateFuncWrapper" };
			var natives = new object[3];
			for (int i = 0; i < 3; i++)
			{
				var type = asm.GetType("GtkSharp." + names[i], throwOnError: true)!;
				_roots[i] = Activator.CreateInstance(type, all, null, new object[] { managed[i] }, null)!;
				natives[i] = type.GetField("NativeDelegate", all)!.GetValue(_roots[i])!;
			}
			_natives = natives;
		}
		catch (Exception ex) when (ex is ArgumentException or MissingMemberException or TypeLoadException
			or InvalidOperationException or TargetInvocationException)
		{
			_create = null;
			_natives = null;
		}
	}

	internal static Gtk.CustomLayout? Create()
	{
		if (_create == null || _natives == null) return null;
		var raw = (IntPtr)_create.DynamicInvoke(_natives)!;
		return raw == IntPtr.Zero ? null : new Gtk.CustomLayout(raw);
	}

	static void Measure(Gtk.Widget widget, Gtk.Orientation orientation, int forSize,
		out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
	{
		minimum = 0;
		minimumBaseline = naturalBaseline = -1;
		natural = Owners.TryGetValue(widget, out var shape) ? shape.NaturalRequest(orientation) : 0;
	}
}
