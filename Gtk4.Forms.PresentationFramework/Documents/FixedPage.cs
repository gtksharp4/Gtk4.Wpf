using System.Windows.Controls;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// A fixed-size page: a panel whose children sit at absolute positions given by the attached
/// <see cref="LeftProperty"/>, <see cref="TopProperty"/>, <see cref="RightProperty"/> and
/// <see cref="BottomProperty"/>. Mirrors WPF <c>FixedPage</c>.
/// </summary>
/// <remarks>
/// Deviation: WPF derives <c>FixedPage</c> straight from <c>FrameworkElement</c>; here it derives from
/// <see cref="Panel"/> (which is a <c>FrameworkElement</c>) to reuse the GTK child hosting. Panel's
/// <c>Background</c> is the page background. <see cref="PrintTicket"/> is stored only.
/// </remarks>
public sealed partial class FixedPage : Panel, IUriContext
{
	Gtk.Fixed? _fixed;
	Uri? _baseUri;

	/// <summary>Creates an empty page.</summary>
	public FixedPage() { }

	/// <summary>Gets or sets the print ticket of the page (stored only; no printing is performed).</summary>
	public object? PrintTicket
	{
		get => GetValue(PrintTicketProperty);
		set => SetValue(PrintTicketProperty, value);
	}

	/// <summary>Identifies the PrintTicket property.</summary>
	public static readonly DependencyProperty PrintTicketProperty = DependencyProperty.Register(
		nameof(PrintTicket), typeof(object), typeof(FixedPage), new PropertyMetadata(null));

	/// <summary>The package part the page was loaded from (as a package URI), or null.</summary>
	internal Uri? SourcePart { get; set; }

	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	internal override bool IsWidgetCreated => _fixed != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_fixed = new Gtk.Fixed();
		foreach (UIElement child in Children)
			PlaceChild(child);
		SyncSize();
		return _fixed;
	}

	/// <inheritdoc/>
	protected override void AddChildWidget(UIElement child) => PlaceChild(child);

	/// <inheritdoc/>
	protected override void RemoveChildWidget(UIElement child) => _fixed!.Remove(child.Widget);

	void PlaceChild(UIElement child)
	{
		var (x, y) = Position(child);
		_fixed!.Put(child.Widget, x, y);
	}

	static (double X, double Y) Position(UIElement child)
	{
		double x = GetLeft(child), y = GetTop(child);
		return (double.IsNaN(x) ? 0 : x, double.IsNaN(y) ? 0 : y);
	}

	void SyncSize()
	{
		if (_fixed == null)
			return;
		_fixed.WidthRequest = double.IsNaN(Width) ? -1 : (int)Math.Ceiling(Width);
		_fixed.HeightRequest = double.IsNaN(Height) ? -1 : (int)Math.Ceiling(Height);
	}

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size availableSize)
	{
		foreach (UIElement child in Children)
			child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		return new Size(0, 0);
	}

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize)
	{
		foreach (UIElement child in Children)
		{
			var (x, y) = Position(child);
			double right = GetRight(child), bottom = GetBottom(child);
			if (double.IsNaN(GetLeft(child)) && !double.IsNaN(right))
				x = finalSize.Width - child.DesiredSize.Width - right;
			if (double.IsNaN(GetTop(child)) && !double.IsNaN(bottom))
				y = finalSize.Height - child.DesiredSize.Height - bottom;
			child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
		}
		return finalSize;
	}
}
