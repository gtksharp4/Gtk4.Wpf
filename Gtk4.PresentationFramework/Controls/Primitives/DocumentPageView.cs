using System.Windows.Documents;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Shows one page of a <see cref="DocumentPaginator"/>. The page number is zero-based. The page is painted on a
/// Cairo drawing area from its <see cref="DrawingVisual"/> (see <see cref="DocumentPageRenderer"/>).
/// </summary>
public class DocumentPageView : FrameworkElement
{
	/// <summary>Identifies the PageNumber property (default 0).</summary>
	public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register(
		nameof(PageNumber), typeof(int), typeof(DocumentPageView),
		new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPageNumberChanged),
		v => v is int n && n >= 0);

	DocumentPaginator? _paginator;
	DocumentPage? _page;
	Gtk.DrawingArea? _area;

	/// <summary>Gets or sets the zero-based page shown.</summary>
	public int PageNumber
	{
		get => (int)GetValue(PageNumberProperty)!;
		set => SetValue(PageNumberProperty, value);
	}

	/// <summary>Gets or sets the paginator that supplies the page.</summary>
	public DocumentPaginator? DocumentPaginator
	{
		get => _paginator;
		set
		{
			if (ReferenceEquals(_paginator, value)) return;
			_paginator = value;
			Reload();
		}
	}

	/// <summary>Gets the page shown; <see cref="DocumentPage.Missing"/> when none.</summary>
	public DocumentPage DocumentPage => _page ?? DocumentPage.Missing;

	/// <summary>Occurs when a page was loaded.</summary>
	public event EventHandler? PageConnected;

	/// <summary>Occurs when the loaded page was released.</summary>
	public event EventHandler? PageDisconnected;

	static void OnPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((DocumentPageView)d).Reload();

	void Reload()
	{
		if (_page != null)
		{
			_page = null;
			PageDisconnected?.Invoke(this, EventArgs.Empty);
		}
		if (_paginator != null && PageNumber < _paginator.PageCount)
		{
			var page = _paginator.GetPage(PageNumber);
			if (!ReferenceEquals(page, DocumentPage.Missing))
			{
				_page = page;
				PageConnected?.Invoke(this, EventArgs.Empty);
			}
		}
		if (_area != null) SyncSize(_area);
	}

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_area = new Gtk.DrawingArea();
		_area.DrawFunc = CairoDrawFunc.Wrap((cr, w, h) =>
		{
			cr.SetSourceRGB(1, 1, 1);
			cr.Paint();
			if (_page != null) DocumentPageRenderer.Render(_page, cr);
		});
		SyncSize(_area);
		return _area;
	}

	void SyncSize(Gtk.DrawingArea area)
	{
		var size = _page?.Size ?? new Size(0, 0);
		bool finite = !double.IsInfinity(size.Width) && !double.IsInfinity(size.Height);
		area.ContentWidth = finite ? (int)size.Width : 0;
		area.ContentHeight = finite ? (int)size.Height : 0;
		area.QueueDraw();
	}
}
