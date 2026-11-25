using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

public partial class FlowDocument
{
	/// <summary>Raised when the blocks, Background, font settings or FlowDirection change.</summary>
	internal event Action? ViewChanged;

	/// <summary>Raised when the blocks or a layout-affecting property change; repaginates.</summary>
	internal event Action? ContentChanged;

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (PaginationProperties.Affects(this, e.Property)) ContentChanged?.Invoke();
		if (e.Property == BackgroundProperty || e.Property == FontStyleProperty || e.Property == FontWeightProperty
			|| e.Property == FontFamilyProperty || e.Property == FontSizeProperty
			|| e.Property == FlowDirectionProperty || e.Property == ColumnRuleBrushProperty)
			RaiseViewChanged();
	}

	bool HasLocal(DependencyProperty dp) => !ReferenceEquals(ReadLocalValue(dp), DependencyProperty.UnsetValue);

	/// <summary>
	/// The markup a viewer shows: <see cref="BuildMarkup"/> wrapped in the document's own FontFamily, FontSize,
	/// FontStyle and FontWeight when they are set locally. <see cref="BuildMarkup"/> itself is unchanged.
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
		if (HasLocal(FontFamilyProperty))
			attrs.Append(" font_family=\"").Append(PangoMarkup.Escape(FontFamily.Source)).Append('"');
		if (HasLocal(FontSizeProperty))
			attrs.Append(" size=\"").Append((int)Math.Round(FontSize * 768)).Append('"');
		return attrs.Length == 0 ? markup : "<span" + attrs + ">" + markup + "</span>";
	}
}
