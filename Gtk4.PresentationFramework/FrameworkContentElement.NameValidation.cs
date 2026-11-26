using System.Globalization;

namespace System.Windows;

public partial class FrameworkContentElement
{
	// WPF NameValidationHelper: null and empty are allowed, otherwise a XAML identifier.
	static bool IsValidName(object? value)
	{
		if (value == null)
			return true;
		if (!(value is string name))
			return false;
		for (int i = 0; i < name.Length; i++)
		{
			var category = char.GetUnicodeCategory(name[i]);
			bool start = name[i] == '_' || category == UnicodeCategory.UppercaseLetter
				|| category == UnicodeCategory.LowercaseLetter || category == UnicodeCategory.TitlecaseLetter
				|| category == UnicodeCategory.OtherLetter || category == UnicodeCategory.LetterNumber;
			bool part = start || category == UnicodeCategory.NonSpacingMark
				|| category == UnicodeCategory.SpacingCombiningMark || category == UnicodeCategory.ModifierLetter
				|| category == UnicodeCategory.DecimalDigitNumber;
			if (!(i == 0 ? start : part))
				return false;
		}
		return true;
	}
}
