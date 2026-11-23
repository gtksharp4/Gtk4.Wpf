using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectBuilder
{
	/// <summary>
	/// The .NET 9 shorthand <c>RowDefinitions="Auto,*,Auto"</c> / <c>ColumnDefinitions="100,2*,*"</c>: each token
	/// (comma and/or whitespace separated) becomes one definition with that GridLength.
	/// </summary>
	static bool TryApplyDefinitionShorthand(object instance, string name, string raw)
	{
		if (!(instance is Grid grid) || raw.TrimStart().StartsWith("{", StringComparison.Ordinal)) return false;
		bool rows = name == "RowDefinitions";
		if (!rows && name != "ColumnDefinitions") return false;
		foreach (string token in Regex.Split(raw.Trim(), @"[\s,]+"))
		{
			if (token.Length == 0) continue;
			var length = (GridLength)XamlValueConverter.ConvertByType(typeof(GridLength), token)!;
			if (rows) grid.RowDefinitions.Add(new RowDefinition { Height = length });
			else grid.ColumnDefinitions.Add(new ColumnDefinition { Width = length });
		}
		return true;
	}
}
