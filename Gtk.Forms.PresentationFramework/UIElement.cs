using System;

namespace System.Windows
{
	/// <summary>
	/// Base class for all WPF visual elements. Each UIElement owns a backing
	/// <see cref="Gtk.Widget"/> created on first access via <see cref="CreateWidget"/>.
	/// </summary>
	public abstract partial class UIElement : DependencyObject
	{
		Gtk.Widget? _widget;

		/// <summary>
		/// Creates the backing GTK widget. Called once; result is cached in <see cref="Widget"/>.
		/// </summary>
		protected abstract Gtk.Widget CreateWidget();

		/// <summary>
		/// The backing GTK widget for this element. Created lazily on first access.
		/// </summary>
		public Gtk.Widget Widget => _widget ??= CreateWidget();

		// ── Measure / Arrange (Phase-0 stubs — GTK drives its own layout) ───────────

		/// <summary>The size this element measured to on the last <see cref="Measure"/> call.</summary>
		public Size DesiredSize { get; protected set; }

		/// <summary>
		/// Phase-0 stub: GTK drives its own layout pass. Records the available size and
		/// delegates to <see cref="MeasureCore"/>.
		/// </summary>
		public void Measure(Size availableSize)
		{
			DesiredSize = MeasureCore(availableSize);
		}

		/// <summary>Override to participate in Phase-1+ two-pass layout.</summary>
		protected virtual Size MeasureCore(Size availableSize) => new Size(0, 0);

		/// <summary>
		/// Phase-0 stub: GTK drives its own layout pass. Delegates to <see cref="ArrangeCore"/>.
		/// </summary>
		public void Arrange(Rect finalRect)
		{
			ArrangeCore(finalRect);
		}

		/// <summary>Override to participate in Phase-1+ two-pass layout.</summary>
		protected virtual void ArrangeCore(Rect finalRect) { }

		// ── Logical parent ───────────────────────────────────────────────────────────

		UIElement? _logicalParent;

		/// <summary>
		/// Logical parent; setting this also updates the DP inheritance parent so
		/// inheriting properties (e.g. DataContext) propagate automatically.
		/// </summary>
		internal UIElement? LogicalParent
		{
			get => _logicalParent;
			set
			{
				if (ReferenceEquals(_logicalParent, value)) return;
				_logicalParent = value;
				// Wire DP inheritance through the same parent.
				InheritanceParent = value;
			}
		}

		// ── Routed events ─────────────────────────────────────────────────────────

		/// <summary>
		/// Adds a handler for a routed event. Phase-0: handler stored in a simple list;
		/// routing walks the logical parent chain.
		/// </summary>
		public void AddHandler(RoutedEvent routedEvent, Delegate handler)
		{
			if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
			if (handler == null) throw new ArgumentNullException(nameof(handler));
			RoutedEventHost.AddHandler(this, routedEvent, handler);
		}

		/// <summary>
		/// Adds a handler, optionally receiving already-handled events.
		/// WPF-compatible 3-argument overload; unmodified WPF code uses this form.
		/// </summary>
		public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
			if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
			if (handler == null) throw new ArgumentNullException(nameof(handler));
			RoutedEventHost.AddHandler(this, routedEvent, handler, handledEventsToo);
		}

		/// <summary>Removes a previously added routed-event handler.</summary>
		public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
		{
			if (routedEvent == null) throw new ArgumentNullException(nameof(routedEvent));
			if (handler == null) throw new ArgumentNullException(nameof(handler));
			RoutedEventHost.RemoveHandler(this, routedEvent, handler);
		}

		/// <summary>
		/// Raises <paramref name="e"/> on this element, routing it according to its strategy.
		/// </summary>
		protected void RaiseEvent(RoutedEventArgs e)
		{
			if (e == null) throw new ArgumentNullException(nameof(e));
			RoutedEventHost.RaiseEvent(this, e);
		}
	}
}
