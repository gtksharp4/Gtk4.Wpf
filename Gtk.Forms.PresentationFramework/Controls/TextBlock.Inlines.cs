using System.Windows.Documents;

namespace System.Windows.Controls
{
	public partial class TextBlock
	{
		readonly InlineCollection _inlines = new InlineCollection();

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
				if (_inlines.Count == 1 && _inlines[0] is Run r &&
					r.Foreground == null && !r.FontWeight.HasValue && !r.FontStyle.HasValue)
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
}
