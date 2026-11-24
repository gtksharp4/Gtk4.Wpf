using System.Collections.Generic;
using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class TextBlock
{
	const int PangoScale = 1024;

	readonly struct InlineBox
	{
		internal InlineBox(int width, int height, int baseline)
		{
			Width = width;
			Height = height;
			Baseline = baseline;
		}

		internal int Width { get; }
		internal int Height { get; }
		internal int Baseline { get; }
	}

	List<InlineBox> _inlineBoxes = new List<InlineBox>();
	int[] _inlineByteIndexes = new int[0];

	// A hosted child's box: its natural GTK size; a TextBlock aligns its first-line baseline, anything else its bottom.
	static InlineBox MeasureBox(UIElement? child)
	{
		if (child == null || child.Visibility == Visibility.Collapsed) return default;
		var w = child.Widget;
		bool wasHidden = !w.Visible;
		if (wasHidden) w.Visible = true;
		try
		{
			w.Measure(Gtk.Orientation.Horizontal, -1, out _, out int width, out _, out _);
			w.Measure(Gtk.Orientation.Vertical, width, out _, out int height, out _, out int baseline);
			return new InlineBox(width, height, child is TextBlock && baseline >= 0 ? baseline : height);
		}
		finally
		{
			if (wasHidden) w.Visible = false;
		}
	}

	// UTF-8 byte offset of every placeholder, the unit Pango attributes use.
	static int[] PlaceholderOffsets(string text)
	{
		var found = new List<int>();
		int bytes = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '￼') found.Add(bytes);
			if (char.IsHighSurrogate(c)) { bytes += 4; i++; }
			else bytes += c < 0x80 ? 1 : c < 0x800 ? 2 : 3;
		}
		return found.ToArray();
	}

	/// <summary>Sizes one Pango shape per placeholder so the label reserves the children's boxes.</summary>
	void ApplyInlineObjects()
	{
		if (_inlineOverlay == null) return;
		_inlineByteIndexes = PlaceholderOffsets(_label.Text ?? string.Empty);
		_inlineBoxes = new List<InlineBox>();
		var list = new Pango.AttrList();
		for (int i = 0; i < _inlineByteIndexes.Length; i++)
		{
			var box = i < _inlineContainers.Count ? MeasureBox(_inlineContainers[i].Child) : default;
			_inlineBoxes.Add(box);
			var rect = new Pango.Rectangle
			{
				X = 0, Y = -box.Baseline * PangoScale, Width = box.Width * PangoScale, Height = box.Height * PangoScale,
			};
			var shape = new Pango.AttrShape(rect, rect)
			{
				StartIndex = (uint)_inlineByteIndexes[i], EndIndex = (uint)_inlineByteIndexes[i] + 3,
			};
			list.Insert(shape);
		}
		_label.Attributes = list;
		_inlineOverlay.QueueAllocate();
	}

	/// <summary>Re-measures the hosted children and rebuilds the placeholders when one changed size.</summary>
	internal void RefreshInlineObjects()
	{
		if (_inlineOverlay == null) return;
		bool changed = _inlineBoxes.Count != _inlineByteIndexes.Length;
		for (int i = 0; !changed && i < _inlineBoxes.Count && i < _inlineContainers.Count; i++)
		{
			var now = MeasureBox(_inlineContainers[i].Child);
			var old = _inlineBoxes[i];
			changed = now.Width != old.Width || now.Height != old.Height || now.Baseline != old.Baseline;
		}
		if (changed) ApplyInlineObjects();
		PlaceInlineChildren();
	}

	// Each child sits at its placeholder's logical rect in the label's layout, shifted by the label's own offsets.
	void PlaceInlineChildren()
	{
		if (_inlineLayer == null || !_label.IsMapped || !_label.ComputeBounds(_inlineOverlay!, out var origin)) return;
		_label.GetLayoutOffsets(out int dx, out int dy);
		var layout = _label.Layout;
		for (int i = 0; i < _inlineByteIndexes.Length && i < _inlineContainers.Count; i++)
		{
			var child = _inlineContainers[i].Child;
			if (child == null || !child.IsWidgetCreated || !ReferenceEquals(child.Widget.Parent, _inlineLayer)) continue;
			var pos = layout.IndexToPos(_inlineByteIndexes[i]);
			double x = Math.Round(origin.X + dx + pos.X / (double)PangoScale);
			double y = Math.Round(origin.Y + dy + pos.Y / (double)PangoScale);
			_inlineLayer.GetChildPosition(child.Widget, out double cx, out double cy);
			if (cx != x || cy != y) _inlineLayer.Move(child.Widget, x, y);
		}
	}

	/// <summary>Test seam: the x of the text before and after placeholder <paramref name="index"/>.</summary>
	internal (double Before, double After) InlineObjectEdges(int index)
	{
		_label.ComputeBounds(_inlineOverlay!, out var origin);
		_label.GetLayoutOffsets(out int dx, out _);
		var pos = _label.Layout.IndexToPos(_inlineByteIndexes[index]);
		double before = origin.X + dx + pos.X / (double)PangoScale;
		return (before, before + pos.Width / (double)PangoScale);
	}

	/// <summary>Test seam: the height of the first layout line.</summary>
	internal double InlineFirstLineHeight
	{
		get
		{
			_label.Layout.GetPixelSize(out _, out int total);
			return total / (double)Math.Max(1, _label.Layout.LineCount);
		}
	}

	/// <summary>Test seam: the label's first-line baseline, from its top.</summary>
	internal int InlineLabelBaseline
	{
		get
		{
			_label.Measure(Gtk.Orientation.Vertical, -1, out _, out _, out _, out int baseline);
			return baseline;
		}
	}
}
