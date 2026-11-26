using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class InkCanvas
{
	/// <summary>Identifies the attached Left property (default NaN).</summary>
	public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached(
		"Left", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnOffsetChanged));

	/// <summary>Identifies the attached Top property (default NaN).</summary>
	public static readonly DependencyProperty TopProperty = DependencyProperty.RegisterAttached(
		"Top", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnOffsetChanged));

	/// <summary>Identifies the attached Right property (default NaN).</summary>
	public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached(
		"Right", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnOffsetChanged));

	/// <summary>Identifies the attached Bottom property (default NaN).</summary>
	public static readonly DependencyProperty BottomProperty = DependencyProperty.RegisterAttached(
		"Bottom", typeof(double), typeof(InkCanvas), new FrameworkPropertyMetadata(double.NaN, OnOffsetChanged));

	readonly Surface _surface = new Surface();

	/// <summary>Creates an ink canvas.</summary>
	public InkCanvas() => _surface.LogicalParent = this;

	/// <summary>The elements hosted on the canvas.</summary>
	public UIElementCollection Children => _surface.Children;

	internal override bool IsWidgetCreated => _surface.IsWidgetCreated;

	internal override IEnumerable<UIElement> VisualChildren => new UIElement[] { _surface };

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget() => _surface.Widget;

	/// <summary>Gets the left offset of <paramref name="element"/>.</summary>
	public static double GetLeft(UIElement element) => (double)element.GetValue(LeftProperty)!;
	/// <summary>Sets the left offset of <paramref name="element"/>.</summary>
	public static void SetLeft(UIElement element, double value) => element.SetValue(LeftProperty, value);
	/// <summary>Gets the top offset of <paramref name="element"/>.</summary>
	public static double GetTop(UIElement element) => (double)element.GetValue(TopProperty)!;
	/// <summary>Sets the top offset of <paramref name="element"/>.</summary>
	public static void SetTop(UIElement element, double value) => element.SetValue(TopProperty, value);
	/// <summary>Gets the right offset of <paramref name="element"/>.</summary>
	public static double GetRight(UIElement element) => (double)element.GetValue(RightProperty)!;
	/// <summary>Sets the right offset of <paramref name="element"/>.</summary>
	public static void SetRight(UIElement element, double value) => element.SetValue(RightProperty, value);
	/// <summary>Gets the bottom offset of <paramref name="element"/>.</summary>
	public static double GetBottom(UIElement element) => (double)element.GetValue(BottomProperty)!;
	/// <summary>Sets the bottom offset of <paramref name="element"/>.</summary>
	public static void SetBottom(UIElement element, double value) => element.SetValue(BottomProperty, value);

	static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UIElement child && child.LogicalParent is Surface) Surface.Mirror(child);
	}

	// The inner Canvas places children; the offsets are mirrored onto its own attached properties.
	sealed class Surface : Canvas
	{
		// The canvas stands in for the Surface in the public tree.
		internal override DependencyObject? ParentOfChildren => LogicalParent;

		protected override void AddChildWidget(UIElement child)
		{
			Mirror(child);
			base.AddChildWidget(child);
		}

		internal static void Mirror(UIElement child)
		{
			SetLeft(child, InkCanvas.GetLeft(child));
			SetTop(child, InkCanvas.GetTop(child));
			SetRight(child, InkCanvas.GetRight(child));
			SetBottom(child, InkCanvas.GetBottom(child));
		}
	}
}
