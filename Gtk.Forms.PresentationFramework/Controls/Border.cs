using System.Windows.Markup;

namespace System.Windows.Controls
{
	/// <summary>
	/// A decorator that draws a border around a single child element,
	/// backed by <see cref="Gtk.Frame"/>.
	/// <see cref="BorderThickness"/> and <see cref="Padding"/> are recorded but
	/// not yet applied to the GTK widget (GTK 4 uses CSS for spacing — Phase-2).
	/// </summary>
	[ContentProperty("Child")]
	public class Border : FrameworkElement
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

		/// <summary>Border thickness (stored; CSS application is Phase-2).</summary>
		public static readonly DependencyProperty BorderThicknessProperty =
			DependencyProperty.Register(
				nameof(BorderThickness),
				typeof(Thickness),
				typeof(Border),
				new PropertyMetadata(new Thickness(0)));

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
			_frame = new Gtk.Frame();
			_frame.Label = null; // no title label — pure border
			if (Child != null)
				_frame.Child = Child.Widget;
			return _frame;
		}
	}
}
