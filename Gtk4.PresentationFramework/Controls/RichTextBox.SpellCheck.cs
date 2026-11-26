namespace System.Windows.Controls;

public partial class RichTextBox
{
	SpellCheck? _spellCheck;

	/// <summary>Gets the spell-check settings of this control (stored only; GTK does not check spelling).</summary>
	public SpellCheck SpellCheck => _spellCheck ??= new SpellCheck(this);
}
