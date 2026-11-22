using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// Paints <see cref="OnRender"/> output and non-UIElement visual children (DrawingVisual and friends) through
/// Cairo on a <see cref="Gtk.DrawingArea"/> layered under the element's widget children.
/// <para>
/// Gaps: the layer exists only on the plain default container UIElement.CreateWidget makes (BinLayout stacks the
/// layer and the children at the same origin). A host with its own widget or layout, such as a Panel, keeps its
/// arrangement and paints no layer; the object model stays exact. All
/// non-UIElement visuals share one layer
/// below the UIElement children, so an "overlay" visual does not paint above them. Clip is stored, not applied.
/// </para>
/// </summary>
public abstract partial class UIElement
{
	static readonly ConcurrentDictionary<Type, bool> _overridesOnRender = new ConcurrentDictionary<Type, bool>();
	static readonly ConcurrentDictionary<Type, bool> _overridesChildCount = new ConcurrentDictionary<Type, bool>();

	Gtk.Box? _defaultBox;
	Gtk.DrawingArea? _renderArea;
	DrawingContext? _renderRecording;
	bool _renderDirty = true;
	List<UIElement>? _pendingHostedChildren;

	/// <summary>Called from <see cref="OnWidgetCreated"/>.</summary>
	partial void AttachRenderLayerOnCreated(Gtk.Widget widget);

	partial void AttachRenderLayerOnCreated(Gtk.Widget widget)
	{
		if (_pendingHostedChildren != null)
		{
			var pending = _pendingHostedChildren;
			_pendingHostedChildren = null;
			foreach (var child in pending) HostChildWidget(child);
		}
		EnsureRenderLayer();
	}

	bool OverridesOnRender => _overridesOnRender.GetOrAdd(GetType(), t =>
		t.GetMethod("OnRender", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
			new[] { typeof(DrawingContext) }, null)?.DeclaringType != typeof(UIElement));

	bool HasNonElementVisualChildren()
	{
		bool overridden = _overridesChildCount.GetOrAdd(GetType(), t =>
			t.GetProperty("VisualChildrenCount", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				?.DeclaringType != typeof(UIElement));
		if (!overridden) return false;
		for (int i = 0; i < VisualChildrenCount; i++)
			if (GetVisualChild(i) is not UIElement) return true;
		return false;
	}

	void EnsureRenderLayer()
	{
		if (_renderArea != null || _widget == null) return;
		if (!OverridesOnRender && !HasNonElementVisualChildren()) return;
		if (!(_widget is Gtk.Box box) || !ReferenceEquals(box, _defaultBox)) return;
		var area = new Gtk.DrawingArea { CanTarget = false };
		area.DrawFunc = OnRenderLayerDraw;
		box.LayoutManager = new Gtk.BinLayout();
		box.Prepend(area);
		_renderArea = area;
	}

	internal bool HasRenderLayer => _renderArea != null;

	internal void InvalidateRenderLayer()
	{
		_renderDirty = true;
		EnsureRenderLayer();
		_renderArea?.QueueDraw();
	}

	void OnRenderLayerDraw(Gtk.DrawingArea area, Cairo.Context cr, int width, int height) => PaintRenderLayer(cr);

	/// <summary>
	/// Runs <see cref="OnRender"/> if invalidated and paints the layer content onto <paramref name="cr"/>.
	/// </summary>
	internal void PaintRenderLayer(Cairo.Context cr)
	{
		try
		{
			if (_renderDirty)
			{
				_renderDirty = false;
				_renderRecording = null;
				if (OverridesOnRender)
				{
					var ctx = new DrawingContext();
					OnRender(ctx);
					ctx.Close();
					_renderRecording = ctx;
				}
			}
			if (_renderRecording != null) DrawingContextRenderer.Render(_renderRecording, cr, paintText: true);
			VisualRenderer.PaintChildren(this, cr);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("Render layer failed: " + ex);
		}
	}

	// ── Child widget hosting for AddVisualChild / VisualCollection ──────────────

	internal override void OnVisualChildAdopted(Visual child)
	{
		if (child is UIElement ue) HostChildWidget(ue);
		else InvalidateRenderLayer();
	}

	internal override void OnVisualChildReleased(Visual child)
	{
		if (child is UIElement ue)
		{
			_pendingHostedChildren?.Remove(ue);
			if (_widget is Gtk.Box box && ue.IsWidgetCreated && ue.Widget.Parent == box) box.Remove(ue.Widget);
		}
		else
		{
			InvalidateRenderLayer();
		}
	}

	void HostChildWidget(UIElement child)
	{
		if (_widget == null) { (_pendingHostedChildren ??= new List<UIElement>()).Add(child); return; }
		if (_widget is not Gtk.Box box) return;
		var w = child.Widget;
		if (w.Parent == null) box.Append(w);
	}
}
