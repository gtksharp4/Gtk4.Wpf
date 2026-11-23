using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Hosts adorners above an element tree. Provides the object model for adding,
/// removing, and querying adorners per adorned element. The static
/// <see cref="GetAdornerLayer(Visual)"/> walks the visual tree upward to locate
/// the nearest layer, mirroring WPF.
/// </summary>
/// <remarks>
/// GTK rendering: when created from an <see cref="AdornerDecorator"/> a
/// <see cref="Gtk.Overlay"/> is used so adorner widgets render above the content
/// (see <c>AdornerLayer.Gtk.cs</c>). Without a realized widget tree the layer
/// tracks adorners logically and does not throw.
/// </remarks>
public sealed partial class AdornerLayer : FrameworkElement
{
	// Key: the adorned UIElement; value: live adorners for that element.
	readonly Dictionary<UIElement, List<Adorner>> _adorners = new();

	// ── Object-model API ──────────────────────────────────────────────────────

	/// <summary>
	/// Adds <paramref name="adorner"/> to this layer.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">
	/// Thrown when <paramref name="adorner"/> is already in this layer.
	/// </exception>
	public void Add(Adorner adorner)
	{
		if (adorner == null) throw new ArgumentNullException(nameof(adorner));
		var key = adorner.AdornedElement;
		if (!_adorners.TryGetValue(key, out var list))
		{
			list = new List<Adorner>();
			_adorners[key] = list;
		}
		if (list.Contains(adorner))
			throw new InvalidOperationException("Adorner is already in this layer.");
		list.Add(adorner);
		adorner.LogicalParent = this;
		OnAdornerAdded(adorner);
	}

	/// <summary>
	/// Removes <paramref name="adorner"/> from this layer.
	/// Does nothing if <paramref name="adorner"/> is not in this layer.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public void Remove(Adorner adorner)
	{
		if (adorner == null) throw new ArgumentNullException(nameof(adorner));
		var key = adorner.AdornedElement;
		if (!_adorners.TryGetValue(key, out var list)) return;
		if (!list.Remove(adorner)) return;
		if (list.Count == 0) _adorners.Remove(key);
		adorner.LogicalParent = null;
		OnAdornerRemoved(adorner);
	}

	/// <summary>
	/// Returns all adorners for <paramref name="element"/>, or <c>null</c> when none.
	/// </summary>
	/// <exception cref="ArgumentNullException"/>
	public Adorner[]? GetAdorners(UIElement element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (_adorners.TryGetValue(element, out var list) && list.Count > 0)
			return list.ToArray();
		return null;
	}

	// ── Static tree walk ──────────────────────────────────────────────────────

	/// <summary>
	/// Walks the visual tree upward from <paramref name="visual"/> and returns
	/// the first <see cref="AdornerLayer"/> found, or <c>null</c> when none.
	/// </summary>
	/// <remarks>
	/// The nearest <see cref="AdornerDecorator"/> ancestor wins; otherwise a containing
	/// <see cref="Window"/> supplies its implicit layer. The window itself has none, as in WPF.
	/// </remarks>
	public static AdornerLayer? GetAdornerLayer(Visual visual)
	{
		if (visual == null) throw new ArgumentNullException(nameof(visual));
		Visual? current = visual.VisualParent;
		while (current != null)
		{
			if (current is AdornerDecorator dec)
				return dec.AdornerLayer;
			// WPF's default Window template carries an AdornerDecorator; see Window.AdornerLayer.cs.
			if (current is Window window)
				return window.ImplicitAdornerLayer;
			current = current.VisualParent;
		}
		return null;
	}

	// Partial methods implemented in AdornerLayer.Gtk.cs for GTK overlay wiring.
	partial void OnAdornerAdded(Adorner adorner);
	partial void OnAdornerRemoved(Adorner adorner);

	// ── GTK widget ───────────────────────────────────────────────────────────

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException(
			"AdornerLayer has no standalone GTK widget. " +
			"Use AdornerDecorator which wires an Overlay.");
}
