namespace System.Windows.Controls;

public partial class TextBox
{
	SpellCheck? _spellCheck;

	/// <summary>Gets the spell-check settings of this control (stored only; GTK does not check spelling).</summary>
	public SpellCheck SpellCheck => _spellCheck ??= new SpellCheck(this);

	/// <summary>Gets the spelling error at <paramref name="charIndex"/>; always null (no spelling engine).</summary>
	public SpellingError? GetSpellingError(int charIndex) => null;

	/// <summary>Gets the length of the error at <paramref name="charIndex"/>; always 0.</summary>
	public int GetSpellingErrorLength(int charIndex) => 0;

	/// <summary>Gets the start of the error containing <paramref name="charIndex"/>; always -1.</summary>
	public int GetSpellingErrorStart(int charIndex) => -1;
}
