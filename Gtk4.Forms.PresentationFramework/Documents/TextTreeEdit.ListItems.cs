using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>The item of <paramref name="list"/> that holds <paramref name="block"/> directly, or null.</summary>
	static ListItem? ItemOf(List list, Block block) =>
		list.ListItems.FirstOrDefault(i => i.Blocks.Any(b => ReferenceEquals(b, block)));

	/// <summary>Removes items whose blocks were all deleted, as WPF merges them away.</summary>
	static void DropEmptyItems(List list, IEnumerable<ListItem> candidates)
	{
		foreach (var item in candidates.ToList())
			if (item.Blocks.Count == 0) list.ListItems.Remove(item);
	}
}
