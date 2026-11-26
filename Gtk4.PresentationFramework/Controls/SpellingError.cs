using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>Describes a misspelled word. No instance is produced: spelling is not checked on GTK.</summary>
public sealed class SpellingError
{
	internal SpellingError() { }

	/// <summary>Gets the suggested corrections.</summary>
	public IEnumerable<string> Suggestions => Array.Empty<string>();

	/// <summary>Replaces the misspelled word with <paramref name="correctedText"/>.</summary>
	public void Correct(string correctedText) { }

	/// <summary>Ignores all occurrences of the misspelled word.</summary>
	public void IgnoreAll() { }
}
