using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base of the paginated viewers. It owns the document, the page count and the current (master) page; the derived
/// viewer supplies the GTK widget. Page numbers here are one-based, as in WPF; <see cref="DocumentPageView"/> is
/// zero-based.
/// </summary>
[ContentProperty("Document")]
public abstract partial class DocumentViewerBase : Control
{
	/// <summary>Identifies the Document property (default null).</summary>
	public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
		nameof(Document), typeof(IDocumentPaginatorSource), typeof(DocumentViewerBase),
		new FrameworkPropertyMetadata(null, OnDocumentPropertyChanged));

	DocumentPaginator? _paginator;

	/// <summary>Gets or sets the content to show.</summary>
	public IDocumentPaginatorSource? Document
	{
		get => (IDocumentPaginatorSource?)GetValue(DocumentProperty);
		set => SetValue(DocumentProperty, value);
	}

	static void OnDocumentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((DocumentViewerBase)d).ChangeDocument(e.OldValue as IDocumentPaginatorSource,
			e.NewValue as IDocumentPaginatorSource);

	void ChangeDocument(IDocumentPaginatorSource? old, IDocumentPaginatorSource? doc)
	{
		if (_paginator != null)
		{
			_paginator.PagesChanged -= OnPaginatorPagesChanged;
			if (_paginator is DynamicDocumentPaginator d) d.PaginationCompleted -= OnPaginationCompleted;
			_paginator = null;
		}
		if (old is FlowDocument oldFlow) oldFlow.SetLogicalParent(null);
		if (doc is FlowDocument flow)
		{
			if (flow.Parent != null && !ReferenceEquals(flow.Parent, this))
				throw new InvalidOperationException("Specified document is already the child of another element.");
			flow.SetLogicalParent(this);
		}
		if (doc != null)
		{
			_paginator = doc.DocumentPaginator;
			_paginator.PagesChanged += OnPaginatorPagesChanged;
			if (_paginator is DynamicDocumentPaginator dyn) dyn.PaginationCompleted += OnPaginationCompleted;
		}
		SetMasterPage(doc == null ? 0 : 1);
		RefreshPages();
		OnDocumentChanged();
	}

	void OnPaginatorPagesChanged(object sender, PagesChangedEventArgs e) => RefreshPages();

	// PagesChanged already refreshed the batch; only a count that moved since then needs another pass.
	void OnPaginationCompleted(object? sender, EventArgs e)
	{
		if ((_paginator?.PageCount ?? 0) != PageCount) RefreshPages();
	}

	/// <summary>Called after <see cref="Document"/> changed and the page state was refreshed.</summary>
	protected virtual void OnDocumentChanged()
	{
	}

	/// <summary>Called after <see cref="MasterPageNumber"/> changed.</summary>
	protected virtual void OnMasterPageNumberChanged()
	{
	}
}
