using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// An ordered collection of <see cref="Block"/> elements owned by a
/// <see cref="FlowDocument"/> or a <see cref="Section"/>.
/// </summary>
public sealed class BlockCollection : IEnumerable<Block>
{
	readonly List<Block> _items = new List<Block>();

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
		_items.Add(block);
		Changed?.Invoke();
	}

	/// <summary>Removes a block from the collection.</summary>
	public void Remove(Block block)
	{
		if (block == null) throw new ArgumentNullException(nameof(block));
		_items.Remove(block);
		Changed?.Invoke();
	}

	/// <summary>Removes all blocks.</summary>
	public void Clear()
	{
		_items.Clear();
		Changed?.Invoke();
	}

	/// <inheritdoc/>
	public IEnumerator<Block> GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}