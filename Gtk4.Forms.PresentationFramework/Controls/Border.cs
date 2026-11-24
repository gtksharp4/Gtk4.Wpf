using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A decorator that draws a border around a single child element,
/// backed by <see cref="Gtk.Frame"/>.
/// BorderBrush, BorderThickness and CornerRadius are rendered as CSS on the Gtk.Frame node (see
/// <c>ElementCssStyle</c>); Padding is stored but not yet applied.
/// </summary>
[ContentProperty("Child")]
public partial class Border : FrameworkElement
{
	// ── Child DP ─────────────────────────────────────────────────────────────

	/// <summary>The single child element.</summary>
	public static readonly DependencyProperty ChildProperty =
		DependencyProperty.Register(
			nameof(Child),
			typeof(UIElement),
			typeof(Border),
			new PropertyMetadata(null, OnChildChanged));

	static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var b = (Border)d;
		if (e.OldValue is UIElement old) old.LogicalParent = null;
		if (e.NewValue is UIElement newEl) newEl.LogicalParent = b;
		if (b._frame == null) return;
		b._frame.Child = (e.NewValue as UIElement)?.Widget;
	}

	/// <summary>Gets or sets the child element.</summary>
	public UIElement? Child
	{
		get => (UIElement?)GetValue(ChildProperty);
		set => SetValue(ChildProperty, value);
	}

	// ── BorderThickness DP ───────────────────────────────────────────────────

	/// <summary>Border thickness (rendered as CSS border-width on the frame).</summary>
	public static readonly DependencyProperty BorderThicknessProperty =
		DependencyProperty.Register(
			nameof(BorderThickness),
			typeof(Thickness),
			typeof(Border),
			new FrameworkPropertyMetadata(new Thickness(0), OnAppearanceChanged));

	/// <summary>Gets or sets the border thickness.</summary>
	public Thickness BorderThickness
	{
		get => (Thickness?)GetValue(BorderThicknessProperty) ?? default;
		set => SetValue(BorderThicknessProperty, value);
	}

	// ── Padding DP ───────────────────────────────────────────────────────────

	/// <summary>Inner padding (stored; CSS application is Phase-2).</summary>
	public static readonly DependencyProperty PaddingProperty =
		DependencyProperty.Register(
			nameof(Padding),
			typeof(Thickness),
			typeof(Border),
			new PropertyMetadata(new Thickness(0)));

	/// <summary>Gets or sets the padding.</summary>
	public Thickness Padding
	{
		get => (Thickness?)GetValue(PaddingProperty) ?? default;
		set => SetValue(PaddingProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Frame _frame = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_frame = new BackdropFrame();
		_frame.Label = null; // no title label — pure border
		if (Child != null)
			_frame.Child = Child.Widget;
		return _frame;
	}

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren =>
		Child != null ? new[] { Child } : Enumerable.Empty<UIElement>();
}