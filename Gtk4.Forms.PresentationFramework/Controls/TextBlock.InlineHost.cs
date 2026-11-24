using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// Hosts the UIElements of <see cref="InlineUIContainer"/>s. With containers present at widget creation the root
/// widget is a <see cref="Gtk.Overlay"/> (label as main child, one overlay child per element); otherwise it stays
/// the plain label. Gap: a container added after a label-rooted widget exists is not hosted (the root cannot be
/// swapped under its GTK parent), and FlowDocument containers stay store-only.
/// </summary>
public partial class TextBlock
{
	Gtk.Overlay? _inlineOverlay;
	readonly List<InlineUIContainer> _inlineContainers = new List<InlineUIContainer>();
	readonly List<UIElement> _hostedChildren = new List<UIElement>();

	/// <summary>Whether container elements are laid out inline (the root widget is the overlay).</summary>
	internal bool HostsInlineObjects => _inlineOverlay != null;

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren => _hostedChildren;

	/// <inheritdoc/>
	protected internal override IEnumerator? LogicalChildren => ((IEnumerable)_inlines).GetEnumerator();

	static void CollectContainers(IEnumerable<Inline> inlines, List<InlineUIContainer> into)
	{
		foreach (var inline in inlines)
		{
			if (inline is InlineUIContainer c) into.Add(c);
			else if (inline is Span span) CollectContainers(span.Inlines, into);
		}
	}

	Gtk.Widget? CreateInlineHost()
	{
		var found = new List<InlineUIContainer>();
		CollectContainers(_inlines, found);
		if (found.Count == 0) return null;
		_inlineOverlay = new Gtk.Overlay { Child = _label };
		_inlineOverlay.AddTickCallback((w, clock) =>
		{
			RefreshInlineObjects();
			PlaceInlineChildren();
			return true;
		});
		return _inlineOverlay;
	}

	/// <summary>Adopts the children of the current containers as logical children and hosts their widgets.</summary>
	void SyncInlineChildren()
	{
		if (_inlineContainers.Count == 0 && _hostedChildren.Count == 0 && _inlines.Count == 0) return;
		var now = new List<InlineUIContainer>();
		CollectContainers(_inlines, now);
		var children = new List<UIElement>();
		foreach (var c in now)
			if (c.Child != null && !ReferenceEquals(c.Child, this) && !children.Contains(c.Child))
				children.Add(c.Child);
		foreach (var old in _hostedChildren.ToArray())
		{
			if (children.Contains(old)) continue;
			_hostedChildren.Remove(old);
			old.InlineContainer = null;
			if (_inlineOverlay != null && ReferenceEquals(old.Widget.Parent, _inlineOverlay))
			{
				_inlineOverlay.RemoveOverlay(old.Widget);
				ResetHostedWidget(old);
			}
			RemoveLogicalChild(old);
		}
		foreach (var c in now)
		{
			var child = c.Child;
			if (child == null || ReferenceEquals(child, this)) continue;
			child.InlineContainer = c;
			if (!_hostedChildren.Contains(child))
			{
				AddLogicalChild(child);
				_hostedChildren.Add(child);
			}
			if (_inlineOverlay == null) continue;
			var w = child.Widget;
			if (w.Parent != null) continue;
			w.Halign = Gtk.Align.Start;
			w.Valign = Gtk.Align.Start;
			_inlineOverlay.AddOverlay(w);
			_inlineOverlay.SetMeasureOverlay(w, false);
		}
		_inlineContainers.Clear();
		_inlineContainers.AddRange(now);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		// A nested TextBlock changed its own size (FontSize, Text, ...): the box in the outer text follows at once.
		if (InlineContainer?.OwningTextBlock is { } outer && outer._inlineOverlay != null) outer.RefreshInlineObjects();
	}
}
