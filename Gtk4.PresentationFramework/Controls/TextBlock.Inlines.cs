using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class TextBlock
{
	readonly InlineCollection _inlines;

	/// <summary>Creates an empty TextBlock.</summary>
	public TextBlock() => _inlines = new InlineCollection(this);

	/// <summary>Creates a TextBlock whose content is <paramref name="inline"/>.</summary>
	public TextBlock(Inline inline) : this()
	{
		if (inline == null) throw new ArgumentNullException(nameof(inline));
		Inlines.Add(inline);
	}

	/// <summary>Re-renders after an inline hosted here changed its content.</summary>
	internal void OnInlineContentChanged() => OnInlinesChanged();

	/// <summary>Re-renders after a Span inside this block was edited; the pointer mirror is left alone.</summary>
	internal void OnSpanContentChanged()
	{
		bool was = _pointerSync;
		_pointerSync = true;
		try { OnInlinesChanged(); }
		finally { _pointerSync = was; }
	}

	// Guard flags to prevent feedback loops between Text DP and Inlines.
	bool _settingFromText;
	bool _settingFromInlines;

	/// <summary>
	/// The collection of <see cref="Inline"/> elements to display.
	/// Adding inlines clears the plain <see cref="Text"/> value.
	/// </summary>
	public InlineCollection Inlines
	{
		get
		{
			// Wire the Changed callback once on first access.
			if (!_inlinesWired)
			{
				_inlines.Changed += OnInlinesChanged;
				_inlinesWired = true;
			}
			return _inlines;
		}
	}

	bool _inlinesWired;

	void OnInlinesChanged()
	{
		if (_settingFromText) return;
		// Reflect the inlines into the Text DP (last-plain-run wins, or empty).
		_settingFromInlines = true;
		try
		{
			// Update Text DP to match: empty when inlines are rich, the run text otherwise.
			var blockDec = TextDecorations;
			if (_inlines.Count == 1 && _inlines[0] is Run r &&
			    !r.HasLocalFormat &&
			    (r.TextDecorations == null || r.TextDecorations.Count == 0) &&
			    (blockDec == null || blockDec.Count == 0))
			{
				SetValue(TextProperty, r.Text);
			}
			else
			{
				SetValue(TextProperty, string.Empty);
			}
		}
		finally { _settingFromInlines = false; }
		ApplyToLabel();
	}
}