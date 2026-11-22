using System;
using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Abstract base class for all adorners. An adorner is bound to a specific
/// <see cref="UIElement"/> (the adorned element) and renders in the
/// <see cref="AdornerLayer"/> above it.
/// </summary>
/// <remarks>
/// Object-model: <see cref="AdornedElement"/> is tracked; layout calls
/// (<see cref="UIElement.MeasureOverride"/> / <see cref="UIElement.ArrangeOverride"/>)
/// default to filling the adorned element's render size.
/// GTK rendering: adorners added to an <see cref="AdornerLayer"/> that is backed
/// by a <see cref="Gtk.Overlay"/> have their widget placed as overlay children.
/// Custom adorner visuals require overriding <see cref="UIElement.CreateWidget"/>
/// (not wired in this release — the overlay slot is reserved but left empty).
/// </remarks>
public abstract partial class Adorner : FrameworkElement
{
	/// <summary>The element this adorner decorates.</summary>
	public UIElement AdornedElement { get; }

	/// <summary>Initialises an adorner bound to <paramref name="adornedElement"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	protected Adorner(UIElement adornedElement)
	{
		if (adornedElement == null) throw new ArgumentNullException(nameof(adornedElement));
		AdornedElement = adornedElement;
	}

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size constraint)
	{
		// Default: fill the adorned element's render size.
		return AdornedElement.DesiredSize == default
			? constraint
			: AdornedElement.DesiredSize;
	}

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize) => finalSize;
}
