using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Shows a <see cref="FlowDocument"/> in a viewing mode of the reader's choice. Backed by the same scrolled Pango
/// label as <see cref="FlowDocumentScrollViewer"/>. Gap: Page and TwoPage render as Scroll (the label has no
/// pagination), so there is always one page: <see cref="PageCount"/> is 1 with a document and 0 without, and the
/// find, print, mode-switch and zoom commands have no UI. The zoom values are stored.
/// </summary>
[ContentProperty("Document")]
public partial class FlowDocumentReader : Control
{
	/// <summary>Identifies the Document property.</summary>
	public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
		nameof(Document), typeof(FlowDocument), typeof(FlowDocumentReader),
		new PropertyMetadata(null, OnDocumentChanged));

	/// <summary>Gets or sets the document to show.</summary>
	public FlowDocument? Document
	{
		get => (FlowDocument?)GetValue(DocumentProperty);
		set => SetValue(DocumentProperty, value);
	}

	FlowDocumentPresenter? _presenter;

	static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var reader = (FlowDocumentReader)d;
		if (e.OldValue is FlowDocument old)
		{
			old.ViewChanged -= reader.Sync;
			old.SetLogicalParent(null);
		}
		if (e.NewValue is FlowDocument doc)
		{
			if (doc.Parent != null && !ReferenceEquals(doc.Parent, reader))
				throw new InvalidOperationException("Specified document is already the child of another element.");
			doc.SetLogicalParent(reader);
			doc.ViewChanged += reader.Sync;
		}
		reader.Sync();
		reader.UpdatePages();
	}

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_presenter = new FlowDocumentPresenter();
		Sync();
		return _presenter.Window;
	}

	void Sync() => _presenter?.Show(Document);
}
