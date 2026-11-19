using System.Globalization;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	// FrameworkElement.Language does not exist yet; WPF's default language is en-US.
	static readonly CultureInfo _defaultLanguage = CultureInfo.GetCultureInfo("en-US");

	/// <summary>Effective binding culture: <c>ConverterCulture ?? target Language</c>.</summary>
	internal static CultureInfo ResolveCulture(CultureInfo? converterCulture)
		=> converterCulture ?? _defaultLanguage;
}
