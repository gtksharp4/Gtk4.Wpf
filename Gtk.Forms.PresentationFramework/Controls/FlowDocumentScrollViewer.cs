using System.Windows.Documents;

namespace System.Windows.Controls
{
	/// <summary>
	/// Displays a <see cref="FlowDocument"/> read-only inside a scrollable area.
	/// Backed by a <see cref="Gtk.ScrolledWindow"/> containing a <see cref="Gtk.Label"/>
	/// (Pango markup rendering).  Editing is not supported.
	/// </summary>
	public class FlowDocumentScrollViewer : Control
	{
		// ── Document DP ──────────────────────────────────────────────────────────

		/// <summary>The flow document to display.</summary>
		public static readonly DependencyProperty DocumentProperty =
			DependencyProperty.Register(
				nameof(Document),
				typeof(FlowDocument),
				typeof(FlowDocumentScrollViewer),
				new PropertyMetadata(null, OnDocumentChanged));

		static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((FlowDocumentScrollViewer)d).SyncLabel();

		/// <summary>Gets or sets the flow document to display.</summary>
		public FlowDocument? Document
		{
			get => (FlowDocument?)GetValue(DocumentProperty);
			set => SetValue(DocumentProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.Label _label = null!;
		Gtk.ScrolledWindow _scrolledWindow = null!;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_label = new Gtk.Label(string.Empty)
			{
				Xalign = 0f,
				Yalign = 0f,
				Wrap = true
			};
			_scrolledWindow = new Gtk.ScrolledWindow();
			_scrolledWindow.Child = _label;
			SyncLabel();
			return _scrolledWindow;
		}

		void SyncLabel()
		{
			if (_label == null) return;
			if (Document == null)
			{
				_label.UseMarkup = false;
				_label.Text = string.Empty;
				return;
			}
			var markup = Document.BuildMarkup();
			_label.UseMarkup = true;
			_label.Markup = markup;
		}
	}
}
