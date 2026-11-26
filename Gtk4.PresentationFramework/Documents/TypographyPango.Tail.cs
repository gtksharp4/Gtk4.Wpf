using System.Text;

namespace System.Windows.Documents;

/// <summary>OpenType feature tags for the Typography properties beyond the original six.</summary>
internal static partial class TypographyPango
{
	static readonly DependencyProperty[] StylisticSets =
	{
		Typography.StylisticSet1Property, Typography.StylisticSet2Property, Typography.StylisticSet3Property,
		Typography.StylisticSet4Property, Typography.StylisticSet5Property, Typography.StylisticSet6Property,
		Typography.StylisticSet7Property, Typography.StylisticSet8Property, Typography.StylisticSet9Property,
		Typography.StylisticSet10Property, Typography.StylisticSet11Property, Typography.StylisticSet12Property,
		Typography.StylisticSet13Property, Typography.StylisticSet14Property, Typography.StylisticSet15Property,
		Typography.StylisticSet16Property, Typography.StylisticSet17Property, Typography.StylisticSet18Property,
		Typography.StylisticSet19Property, Typography.StylisticSet20Property,
	};

	/// <summary>Tokens for the tail properties; empty when all are at their WPF defaults.</summary>
	internal static string BuildTailFeatures(DependencyObject e)
	{
		var sb = new StringBuilder();
		for (int i = 0; i < StylisticSets.Length; i++)
			if ((bool)e.GetValue(StylisticSets[i])!)
				Append(sb, "ss" + (i + 1).ToString("00") + "=1");
		Int(sb, "swsh", Typography.GetStandardSwashes(e));
		Int(sb, "cswh", Typography.GetContextualSwashes(e));
		Int(sb, "salt", Typography.GetStylisticAlternates(e));
		Int(sb, "nalt", Typography.GetAnnotationAlternates(e));
		if (!Typography.GetContextualLigatures(e)) Append(sb, "clig=0");
		if (Typography.GetDiscretionaryLigatures(e)) Append(sb, "dlig=1");
		if (Typography.GetHistoricalLigatures(e)) Append(sb, "hlig=1");
		if (!Typography.GetContextualAlternates(e)) Append(sb, "calt=0");
		if (Typography.GetHistoricalForms(e)) Append(sb, "hist=1");
		if (Typography.GetCaseSensitiveForms(e)) Append(sb, "case=1");
		if (Typography.GetCapitalSpacing(e)) Append(sb, "cpsp=1");
		if (Typography.GetSlashedZero(e)) Append(sb, "zero=1");
		if (Typography.GetMathematicalGreek(e)) Append(sb, "mgrk=1");
		if (Typography.GetEastAsianExpertForms(e)) Append(sb, "expt=1");
		Tag(sb, NumeralAlignmentTag(Typography.GetNumeralAlignment(e)));
		Tag(sb, EastAsianWidthsTag(Typography.GetEastAsianWidths(e)));
		Tag(sb, EastAsianLanguageTag(Typography.GetEastAsianLanguage(e)));
		return sb.ToString();
	}

	static void Int(StringBuilder sb, string tag, int value)
	{
		if (value > 0) Append(sb, tag + "=" + value);
	}

	static void Tag(StringBuilder sb, string? tag)
	{
		if (tag != null) Append(sb, tag + "=1");
	}

	static string? NumeralAlignmentTag(FontNumeralAlignment v) =>
		v == FontNumeralAlignment.Proportional ? "pnum" : v == FontNumeralAlignment.Tabular ? "tnum" : null;

	static string? EastAsianWidthsTag(FontEastAsianWidths v)
	{
		switch (v)
		{
			case FontEastAsianWidths.Proportional: return "pwid";
			case FontEastAsianWidths.Full: return "fwid";
			case FontEastAsianWidths.Half: return "hwid";
			case FontEastAsianWidths.Third: return "twid";
			case FontEastAsianWidths.Quarter: return "qwid";
			default: return null;
		}
	}

	static string? EastAsianLanguageTag(FontEastAsianLanguage v)
	{
		switch (v)
		{
			case FontEastAsianLanguage.Jis78: return "jp78";
			case FontEastAsianLanguage.Jis83: return "jp83";
			case FontEastAsianLanguage.Jis90: return "jp90";
			case FontEastAsianLanguage.Jis04: return "jp04";
			case FontEastAsianLanguage.HojoKanji: return "hojo";
			case FontEastAsianLanguage.NlcKanji: return "nlck";
			case FontEastAsianLanguage.Simplified: return "smpl";
			case FontEastAsianLanguage.Traditional: return "trad";
			case FontEastAsianLanguage.TraditionalNames: return "tnam";
			default: return null;
		}
	}
}
