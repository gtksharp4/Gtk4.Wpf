using System.Linq;

namespace System.Windows;

public partial class DependencyObject
{
	/// <summary>Returns a snapshot enumerator over the properties that currently have a local value.</summary>
	public LocalValueEnumerator GetLocalValueEnumerator() =>
		new LocalValueEnumerator(_localValues.Select(kv => new LocalValueEntry(kv.Key, kv.Value)).ToArray());
}
