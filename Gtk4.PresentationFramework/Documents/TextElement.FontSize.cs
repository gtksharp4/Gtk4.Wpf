using System.Globalization;

namespace System.Windows.Documents;

public abstract partial class TextElement
{
	/// <summary>
	/// Gets or sets the font size in DIPs, backed by the attached <see cref="FontSizeProperty"/> (default 12,
	/// inherits). Rendered as a Pango <c>size</c> span when set locally.
	/// </summary>
	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty)!;
		set => SetValue(FontSizeProperty, value);
	}

	// Pango sizes are 1/1024 pt; a DIP is 0.75 pt.
	internal string? PangoSizeAttribute() =>
		ReferenceEquals(ReadLocalValue(FontSizeProperty), DependencyProperty.UnsetValue)
			? null
			: "size=\"" + ((int)Math.Round(FontSize * 768)).ToString(CultureInfo.InvariantCulture) + "\"";
}
