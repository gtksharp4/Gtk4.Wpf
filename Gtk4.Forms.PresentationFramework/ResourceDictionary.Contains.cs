using System;

namespace System.Windows;

public partial class ResourceDictionary
{
	/// <summary>Whether the dictionary, or one of its merged dictionaries, holds <paramref name="key"/>.</summary>
	public bool Contains(object key) => ContainsKey(key);
}
