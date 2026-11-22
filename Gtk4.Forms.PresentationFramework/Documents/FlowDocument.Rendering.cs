using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>Raised when the blocks, Background, FontStyle or FontWeight change.</summary>
	internal event Action? ViewChanged;

	/// <summary>Raised when the blocks or any property change; the paginator repaginates on it.</summary>
	internal event Action? ContentChanged;

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		ContentChanged?.Invoke();
		if (e.Property == BackgroundProperty || e.Property == FontStyleProperty || e.Property == FontWeightProperty)
			ViewChanged?.Invoke();
	}

	bool HasLocal(DependencyProperty dp) => !ReferenceEquals(ReadLocalValue(dp), DependencyProperty.UnsetValue);

	/// <summary>
	/// The markup a viewer shows: <see cref="BuildMarkup"/> wrapped in the document's own FontStyle and FontWeight
	/// when they are set locally. <see cref="BuildMarkup"/> itself is unchanged.
	/// </summary>
	internal string BuildViewMarkup()
	{
		string markup = BuildMarkup();
		var attrs = new StringBuilder();
		if (HasLocal(FontStyleProperty))
			attrs.Append(" style=\"").Append(FontStyle == FontStyles.Italic ? "italic"
				: FontStyle == FontStyles.Oblique ? "oblique" : "normal").Append('"');
		if (HasLocal(FontWeightProperty))
			attrs.Append(" weight=\"").Append(FontWeight.Weight).Append('"');
		return attrs.Length == 0 ? markup : "<span" + attrs + ">" + markup + "</span>";
	}
}
