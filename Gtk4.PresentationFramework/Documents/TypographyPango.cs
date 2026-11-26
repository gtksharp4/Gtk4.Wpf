namespace System.Windows.Documents;

/// <summary>
/// Pure, GTK-free helper that converts <see cref="Typography"/> attached-property values
/// into a Pango <c>font_features='…'</c> attribute string.
/// <para>
/// <b>Pango version requirement:</b> the <c>font_features</c> span attribute requires
/// Pango ≥ 1.38 (released 2014). GTK 4 ships with Pango ≥ 1.50, so this feature is
/// always available on supported GTK 4 runtimes. On runtimes that do not support the
/// attribute, Pango silently ignores the unknown attribute; no runtime error occurs.
/// </para>
/// </summary>
internal static partial class TypographyPango
{
	/// <summary>
	/// Builds the Pango <c>font_features</c> token list from the Typography attached-property
	/// values on <paramref name="element"/>.
	/// Returns an empty string when all values are at their WPF defaults
	/// (no <c>font_features</c> attribute is needed).
	/// </summary>
	/// <param name="element">The <see cref="DependencyObject"/> to read Typography DPs from.</param>
	internal static string BuildFontFeatures(DependencyObject element)
	{
		var capitals = Typography.GetCapitals(element);
		var numeralStyle = Typography.GetNumeralStyle(element);
		var fraction = Typography.GetFraction(element);
		bool stdLigatures = Typography.GetStandardLigatures(element);
		bool kerning = Typography.GetKerning(element);
		var variants = Typography.GetVariants(element);

		var basic = BuildFontFeatures(capitals, numeralStyle, fraction, stdLigatures, kerning, variants);
		var tail = BuildTailFeatures(element);
		return basic.Length == 0 ? tail : tail.Length == 0 ? basic : basic + "," + tail;
	}

	/// <summary>
	/// Builds the Pango <c>font_features</c> token list from explicit values.
	/// Returns an empty string when all values are at their WPF defaults.
	/// </summary>
	internal static string BuildFontFeatures(
		FontCapitals capitals,
		FontNumeralStyle numeralStyle,
		FontFraction fraction,
		bool standardLigatures,
		bool kerning,
		FontVariants variants)
	{
		var sb = new System.Text.StringBuilder();

		AppendCapitals(sb, capitals);
		AppendNumeralStyle(sb, numeralStyle);
		AppendFraction(sb, fraction);

		// StandardLigatures default is true in WPF — only emit when disabled.
		if (!standardLigatures)
			Append(sb, "liga=0");

		// Kerning default is true in WPF — only emit when disabled.
		if (!kerning)
			Append(sb, "kern=0");

		AppendVariants(sb, variants);

		return sb.ToString();
	}

	static void AppendCapitals(System.Text.StringBuilder sb, FontCapitals capitals)
	{
		switch (capitals)
		{
			case FontCapitals.SmallCaps:
				Append(sb, "smcp=1");
				break;
			case FontCapitals.AllSmallCaps:
				Append(sb, "c2sc=1");
				Append(sb, "smcp=1");
				break;
			case FontCapitals.PetiteCaps:
				Append(sb, "pcap=1");
				break;
			case FontCapitals.AllPetiteCaps:
				Append(sb, "c2pc=1");
				Append(sb, "pcap=1");
				break;
			case FontCapitals.Unicase:
				Append(sb, "unic=1");
				break;
			case FontCapitals.Titling:
				Append(sb, "titl=1");
				break;
			// FontCapitals.Normal → no feature token
		}
	}

	static void AppendNumeralStyle(System.Text.StringBuilder sb, FontNumeralStyle style)
	{
		switch (style)
		{
			case FontNumeralStyle.Lining:
				Append(sb, "lnum=1");
				break;
			case FontNumeralStyle.OldStyle:
				Append(sb, "onum=1");
				break;
			// FontNumeralStyle.Normal → no feature token
		}
	}

	static void AppendFraction(System.Text.StringBuilder sb, FontFraction fraction)
	{
		switch (fraction)
		{
			case FontFraction.Slashed:
				Append(sb, "frac=1");
				break;
			case FontFraction.Stacked:
				Append(sb, "afrc=1");
				break;
			// FontFraction.Normal → no feature token
		}
	}

	static void AppendVariants(System.Text.StringBuilder sb, FontVariants variants)
	{
		switch (variants)
		{
			case FontVariants.Superscript:
				Append(sb, "sups=1");
				break;
			case FontVariants.Subscript:
				Append(sb, "subs=1");
				break;
			case FontVariants.Ordinal:
				Append(sb, "ordn=1");
				break;
			case FontVariants.Inferior:
				Append(sb, "sinf=1");
				break;
			case FontVariants.Ruby:
				Append(sb, "ruby=1");
				break;
			// FontVariants.Normal → no feature token
		}
	}

	/// <summary>
	/// Wraps <paramref name="content"/> in a Pango <c>&lt;span font_features='…'&gt;</c>
	/// element when <paramref name="element"/> has any non-default Typography property set.
	/// Returns <paramref name="content"/> unchanged when no features are active.
	/// </summary>
	/// <param name="element">The element to read Typography DPs from.</param>
	/// <param name="content">Already-escaped / markup-safe inner content.</param>
	internal static string WrapWithFontFeatures(DependencyObject element, string content)
	{
		string features = BuildFontFeatures(element);
		if (features.Length == 0)
			return content;
		return $"<span font_features='{features}'>{content}</span>";
	}

	static void Append(System.Text.StringBuilder sb, string token)
	{
		if (sb.Length > 0)
			sb.Append(',');
		sb.Append(token);
	}
}
