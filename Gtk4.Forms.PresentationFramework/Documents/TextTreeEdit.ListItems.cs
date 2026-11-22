using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Documents;

static partial class TextTreeEdit
{
	/// <summary>
	/// The item, at any depth of <paramref name="doc"/>, that holds <paramref name="block"/> directly, with its List.
	/// </summary>
	static ListItem? ItemOwning(FlowDocument doc, Block block, out List? list)
	{
		list = null;
		if (doc.DetachedRoot is List dl) return ItemOwning(dl, block, out list);
		var root = TextTree.RootBlocks(doc, out _);
		return root == null ? null : ItemOwning(root, block, out list);
	}

	static ListItem? ItemOwning(List owner, Block block, out List? list)
	{
		foreach (var item in owner.ListItems)
		{
			if (item.Blocks.Any(b => ReferenceEquals(b, block))) { list = owner; return item; }
			var found = ItemOwning(item.Blocks, block, out list);
			if (found != null) return found;
		}
		list = null;
		return null;
	}

	static ListItem? ItemOwning(BlockCollection col, Block block, out List? list)
	{
		list = null;
		foreach (var b in col)
		{
			ListItem? found = null;
			if (b is Section s) found = ItemOwning(s.Blocks, block, out list);
			else if (b is List l) found = ItemOwning(l, block, out list);
			if (found != null) return found;
		}
		list = null;
		return null;
	}

	/// <summary>Removes items whose blocks were all deleted, as WPF merges them away.</summary>
	static void DropEmptyItems(List list, IEnumerable<ListItem> candidates)
	{
		foreach (var item in candidates.ToList())
			if (item.Blocks.Count == 0) list.ListItems.Remove(item);
	}
}
