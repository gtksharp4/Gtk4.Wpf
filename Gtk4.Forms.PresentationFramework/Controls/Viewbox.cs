using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>Specifies which stretch directions are allowed.</summary>
public enum StretchDirection
{
	/// <summary>Always stretch.</summary>
	Both,
	/// <summary>Only scale up.</summary>
	UpOnly,
	/// <summary>Only scale down.</summary>
	DownOnly,
}

/// <summary>
/// A decorator that scales its single <see cref="Child"/> to fill available space.
/// <para>
/// GTK4 backing: <see cref="Gtk.AspectFrame"/> (preserves aspect ratio for
/// <see cref="Stretch.Uniform"/> and <see cref="Stretch.UniformToFill"/>).
/// For <see cref="Stretch.None"/> and <see cref="Stretch.Fill"/> a plain
/// <see cref="Gtk.Box"/> is used (GTK handles fill natively via expand flags).
/// </para>
/// </summary>
[ContentProperty("Child")]
public class Viewbox : Control
{
	// ── Child DP ─────────────────────────────────────────────────────────────

	/// <summary>The single child element scaled by this Viewbox.</summary>
	public static readonly DependencyProperty ChildProperty =
		DependencyProperty.Register(
			nameof(Child),
			typeof(UIElement),
			typeof(Viewbox),
			new PropertyMetadata(null, OnChildChanged));

	static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Viewbox)d).UpdateChild(e.OldValue as UIElement, e.NewValue as UIElement);
	}

	/// <summary>Gets or sets the child element.</summary>
	public UIElement? Child
	{
		get => (UIElement?)GetValue(ChildProperty);
		set => SetValue(ChildProperty, value);
	}

	// ── Stretch DP ───────────────────────────────────────────────────────────

	/// <summary>How the child is scaled.</summary>
	public static readonly DependencyProperty StretchProperty =
		DependencyProperty.Register(
			nameof(Stretch),
			typeof(Stretch),
			typeof(Viewbox),
			new PropertyMetadata(Stretch.Uniform));

	/// <summary>Gets or sets the stretch mode.</summary>
	public Stretch Stretch
	{
		get => (Stretch?)GetValue(StretchProperty) ?? Stretch.Uniform;
		set => SetValue(StretchProperty, value);
	}

	// ── StretchDirection DP ──────────────────────────────────────────────────

	/// <summary>Which scale directions are permitted.</summary>
	public static readonly DependencyProperty StretchDirectionProperty =
		DependencyProperty.Register(
			nameof(StretchDirection),
			typeof(StretchDirection),
			typeof(Viewbox),
			new PropertyMetadata(StretchDirection.Both));

	/// <summary>Gets or sets the stretch direction.</summary>
	public StretchDirection StretchDirection
	{
		get => (StretchDirection?)GetValue(StretchDirectionProperty) ?? StretchDirection.Both;
		set => SetValue(StretchDirectionProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.AspectFrame? _frame;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		// ratio=1 is a placeholder; GTK4 AspectFrame will keep aspect ratio.
		_frame = new Gtk.AspectFrame(0.5f, 0.5f, 1.0f, false);
		var child = Child;
		if (child != null)
		{
			child.LogicalParent = this;
			_frame.Child = child.Widget;
		}
		return _frame;
	}

	void UpdateChild(UIElement? oldChild, UIElement? newChild)
	{
		if (oldChild != null) oldChild.LogicalParent = null;
		if (newChild != null) newChild.LogicalParent = this;

		if (_frame == null) return;

		if (oldChild != null)
			_frame.Child = null;

		if (newChild != null)
			_frame.Child = newChild.Widget;
	}
}