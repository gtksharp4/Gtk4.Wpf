namespace System.Windows.Documents;

public partial class FlowDocument
{
	int _contentVersion;
	int _symbolsVersion = -1;
	System.Collections.Generic.List<TextSym>? _symbols;

	/// <summary>Marks the text tree as changed so the cached symbol list is rebuilt on next use.</summary>
	internal void InvalidateSymbols() => _contentVersion++;

	/// <summary>
	/// The flattened symbol list, cached until the content changes (collection edits, Run.Text, logged text edits).
	/// A detached root is never cached: its edits do not reach this container.
	/// </summary>
	internal System.Collections.Generic.List<TextSym> Symbols
	{
		get
		{
			if (_symbols == null || _symbolsVersion != _contentVersion || DetachedRoot != null)
			{
				_symbols = TextTree.Build(this);
				_symbolsVersion = _contentVersion;
			}
			return _symbols;
		}
	}

	/// <summary>The total symbol count of the document.</summary>
	internal int SymbolTotal => TextTree.Total(Symbols);
}
