using System;

namespace System.Windows
{
	public partial class FrameworkElement
	{
		// ── Style DP ────────────────────────────────────────────────────────────

		/// <summary>Identifies the <see cref="Style"/> dependency property.</summary>
		public static readonly DependencyProperty StyleProperty =
			DependencyProperty.Register(
				nameof(Style),
				typeof(Style),
				typeof(FrameworkElement),
				new PropertyMetadata(null, OnStyleChanged));

		static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FrameworkElement fe)
				(e.NewValue as Style)?.Apply(fe);
		}

		/// <summary>Gets or sets the style applied to this element.</summary>
		public Style? Style
		{
			get => (Style?)GetValue(StyleProperty);
			set => SetValue(StyleProperty, value);
		}

		// ── Resource lookup ─────────────────────────────────────────────────────

		/// <summary>
		/// Walks up the logical parent chain checking each element's
		/// <see cref="FrameworkElement.Resources"/>, then
		/// <see cref="Application.Current"/>?.Resources.
		/// Returns the resource or throws <see cref="InvalidOperationException"/>.
		/// </summary>
		public object FindResource(object key)
		{
			var v = TryFindResource(key);
			if (v == null)
				throw new InvalidOperationException(
					$"Resource with key '{key}' not found.");
			return v;
		}

		/// <summary>
		/// Same walk as <see cref="FindResource"/>, returns <c>null</c> when absent.
		/// </summary>
		public object? TryFindResource(object key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));

			// Walk the logical parent chain.
			DependencyObject? current = this;
			while (current != null)
			{
				if (current is FrameworkElement fe && fe.Resources.TryGetValue(key, out var v))
					return v;
				current = current.InheritanceParent;
			}

			// Fall back to Application.Current.Resources.
			if (Application.Current?.Resources.TryGetValue(key, out var appVal) == true)
				return appVal;

			return null;
		}

		// ── Implicit style application ───────────────────────────────────────────

		/// <summary>
		/// Looks up an implicit style keyed by <c>GetType()</c> in the resource tree
		/// and applies it when no explicit <see cref="Style"/> is set.
		/// Call from <c>InitializeComponent</c> or after the element is added to the tree.
		/// </summary>
		internal void ApplyImplicitStyle()
		{
			if (GetValue(StyleProperty) != null) return; // explicit style wins
			var implicit_ = TryFindResource(GetType());
			if (implicit_ is Style s)
				Style = s;
		}
	}
}
