namespace System.Windows.Documents;

/// <summary>One page produced by a <see cref="DocumentPaginator"/>: a visual and its boxes.</summary>
public class DocumentPage : IDisposable
{
	/// <summary>The page that stands for "no page": no visual and a zero size.</summary>
	public static readonly DocumentPage Missing = new DocumentPage(null!, new Size(0, 0), Rect.Empty, Rect.Empty);

	Media.Visual? _visual;
	Size _size;
	Rect _bleedBox;
	Rect _contentBox;
	bool _disposed;

	/// <summary>Creates a page around <paramref name="visual"/> with an empty size and boxes.</summary>
	public DocumentPage(Media.Visual visual) : this(visual, Size.Empty, Rect.Empty, Rect.Empty)
	{
	}

	/// <summary>Creates a page with its size, bleed box and content box.</summary>
	public DocumentPage(Media.Visual visual, Size pageSize, Rect bleedBox, Rect contentBox)
	{
		_visual = visual;
		_size = pageSize;
		_bleedBox = bleedBox;
		_contentBox = contentBox;
	}

	/// <summary>Gets the visual that draws the page; null once the page is disposed.</summary>
	public virtual Media.Visual? Visual => _visual;

	/// <summary>Gets the page size.</summary>
	public virtual Size Size => _size;

	/// <summary>Gets the area that receives bleed, including the content.</summary>
	public virtual Rect BleedBox => _bleedBox;

	/// <summary>Gets the area that holds the page content.</summary>
	public virtual Rect ContentBox => _contentBox;

	/// <summary>Occurs once, when the page is disposed.</summary>
	public event EventHandler? PageDestroyed;

	/// <summary>Alias of <see cref="PageDestroyed"/>: occurs once, when the page is disposed.</summary>
	public event EventHandler? Disposed
	{
		add => PageDestroyed += value;
		remove => PageDestroyed -= value;
	}

	/// <summary>Releases the visual and raises <see cref="PageDestroyed"/>; later calls do nothing.</summary>
	public virtual void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		if (!ReferenceEquals(this, Missing)) _visual = null;
		OnPageDestroyed(EventArgs.Empty);
		GC.SuppressFinalize(this);
	}

	/// <summary>Raises <see cref="PageDestroyed"/>.</summary>
	protected virtual void OnPageDestroyed(EventArgs e) => PageDestroyed?.Invoke(this, e);

	/// <summary>Sets <see cref="Visual"/>.</summary>
	protected void SetVisual(Media.Visual? visual) => _visual = visual;

	/// <summary>Sets <see cref="Size"/>.</summary>
	protected void SetSize(Size size) => _size = size;

	/// <summary>Sets <see cref="BleedBox"/>.</summary>
	protected void SetBleedBox(Rect bleedBox) => _bleedBox = bleedBox;

	/// <summary>Sets <see cref="ContentBox"/>.</summary>
	protected void SetContentBox(Rect contentBox) => _contentBox = contentBox;
}
