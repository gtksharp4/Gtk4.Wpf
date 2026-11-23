using System.Collections;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;

namespace System.Windows.Documents;

/// <summary>
/// One page of a <see cref="FixedDocument"/>: either a <see cref="FixedPage"/> held in <see cref="Child"/> or a
/// <see cref="Source"/> URI that is loaded on demand by <see cref="GetPageRoot"/>. Mirrors WPF <c>PageContent</c>.
/// </summary>
/// <remarks>
/// Gaps: the async loading API (<c>GetPageRootAsync</c>) and <c>LinkTargets</c> are not implemented.
/// </remarks>
[ContentProperty("Child")]
public sealed class PageContent : FrameworkElement, IAddChild, IUriContext
{
	/// <summary>Identifies the Source property.</summary>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
		nameof(Source), typeof(Uri), typeof(PageContent), new PropertyMetadata(null, OnSourceChanged));

	FixedPage? _child;
	Uri? _baseUri;

	/// <summary>Creates a page that is empty until <see cref="Source"/> or a child is set.</summary>
	public PageContent() { }

	/// <summary>The URI of the page part, relative to the base URI of the document that holds it.</summary>
	public Uri? Source
	{
		get => (Uri?)GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	/// <summary>
	/// The page, or null when it has not been loaded or added. Setting it replaces the logical child.
	/// </summary>
	/// <exception cref="InvalidOperationException">The page content has a <see cref="Source"/>.</exception>
	public FixedPage? Child
	{
		get => _child;
		set
		{
			if (Source != null)
				throw new InvalidOperationException("A PageContent can have only one page.");
			SetChild(value);
		}
	}

	/// <inheritdoc/>
	protected internal override IEnumerator LogicalChildren =>
		(_child == null ? Array.Empty<object>() : new object[] { _child }).GetEnumerator();

	void SetChild(FixedPage? page)
	{
		if (ReferenceEquals(_child, page))
			return;
		if (_child != null)
			_child.LogicalParent = null;
		_child = page;
		if (page != null)
			page.LogicalParent = this;
	}

	Uri? IUriContext.BaseUri
	{
		get => _baseUri;
		set => _baseUri = value;
	}

	static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var content = (PageContent)d;
		if (content._child?.SourcePart != null)
			content.SetChild(null);
	}

	/// <summary>
	/// Returns the page, loading it from <see cref="Source"/> when it is not loaded yet or when
	/// <paramref name="forceReload"/> is true. Returns null when there is neither a child nor a source.
	/// </summary>
	/// <exception cref="InvalidOperationException">The source cannot be resolved against the base URI.</exception>
	public FixedPage? GetPageRoot(bool forceReload)
	{
		if (_child != null && !(forceReload && Source != null))
			return _child;
		var source = Source;
		if (source == null)
			return _child;
		SetChild(FixedPartLoader.LoadPage(FixedPartLoader.Resolve(_baseUri, source)));
		return _child;
	}

	void IAddChild.AddChild(object value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));
		if (!(value is FixedPage page))
			throw new ArgumentException("A PageContent holds a FixedPage, not " + value.GetType().FullName + ".",
				nameof(value));
		if (_child != null || Source != null)
			throw new InvalidOperationException("A PageContent can have only one page.");
		SetChild(page);
	}

	void IAddChild.AddText(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
			throw new ArgumentException("A PageContent does not accept text.", nameof(text));
	}

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget() => new Gtk.Box(Gtk.Orientation.Vertical, 0);
}
