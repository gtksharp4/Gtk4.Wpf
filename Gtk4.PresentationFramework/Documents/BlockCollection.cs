using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// An ordered collection of <see cref="Block"/> elements owned by a
/// <see cref="FlowDocument"/>, a <see cref="Section"/> or a <see cref="TableCell"/>. An owned collection sets the
/// logical parent of its blocks (a block that already has a parent is re-parented, as before).
/// </summary>
public sealed partial class BlockCollection : IEnumerable<Block>
{
	readonly List<Block> _items = new List<Block>();
	readonly DependencyObject? _owner;

	/// <summary>Creates a collection with no owner.</summary>
	public BlockCollection() { }

	internal BlockCollection(DependencyObject owner) => _owner = owner;

	/// <summary>Raised when the collection is modified.</summary>
	internal event Action? Changed;

	/// <summary>Number of blocks in the collection.</summary>
	public int Count => _items.Count;

	/// <summary>Gets the block at the given index.</summary>
	public Block this[int index] => _items[index];

	/// <summary>Adds a block to the end of the collection.</summary>
	public void Add(Block block)
	{
		if (block == null) throw new ArgumentNullException(nameof(block));
		ContentParenting.Adopt(_owner, block, strict: false);
		_items.Add(block);
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <summary>Removes a block from the collection.</summary>
	public void Remove(Block block)
	{
		if (block == null) throw new ArgumentNullException(nameof(block));
		if (_items.Remove(block)) ContentParenting.Release(_owner, block);
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <summary>Removes all blocks.</summary>
	public void Clear()
	{
		foreach (var block in _items) ContentParenting.Release(_owner, block);
		_items.Clear();
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <inheritdoc/>
	public IEnumerator<Block> GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
