using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// An ordered collection of <see cref="Inline"/> elements that belong to a single
/// <see cref="Controls.TextBlock"/>, <see cref="Paragraph"/> or <see cref="Span"/>. An owned collection
/// (Paragraph, Span) sets the logical parent of its inlines (an inline that already has a parent is re-parented).
/// </summary>
public sealed class InlineCollection : IEnumerable<Inline>
{
	readonly List<Inline> _items = new List<Inline>();
	readonly DependencyObject? _owner;

	/// <summary>Creates a collection with no owner.</summary>
	public InlineCollection() { }

	internal InlineCollection(DependencyObject owner) => _owner = owner;

	/// <summary>Raised when the collection is modified.</summary>
	internal event Action? Changed;

	/// <summary>Number of inlines in the collection.</summary>
	public int Count => _items.Count;

	/// <summary>Gets the inline at the given index.</summary>
	public Inline this[int index] => _items[index];

	/// <summary>Adds an inline to the end of the collection.</summary>
	public void Add(Inline inline)
	{
		if (inline == null) throw new ArgumentNullException(nameof(inline));
		ContentParenting.Adopt(_owner, inline, strict: false);
		_items.Add(inline);
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <summary>Adds a <see cref="Run"/> holding <paramref name="text"/>.</summary>
	public void Add(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));
		Add(new Run(text));
	}

	/// <summary>Adds an <see cref="InlineUIContainer"/> holding <paramref name="uiElement"/>.</summary>
	public void Add(UIElement uiElement)
	{
		if (uiElement == null) throw new ArgumentNullException(nameof(uiElement));
		Add(new InlineUIContainer(uiElement));
	}

	/// <summary>Removes an inline from the collection.</summary>
	public void Remove(Inline inline)
	{
		if (inline == null) throw new ArgumentNullException(nameof(inline));
		if (_items.Remove(inline)) ContentParenting.Release(_owner, inline);
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <summary>Removes all inlines.</summary>
	public void Clear()
	{
		foreach (var inline in _items) ContentParenting.Release(_owner, inline);
		_items.Clear();
		Changed?.Invoke();
		ContentParenting.Changed(_owner);
	}

	/// <inheritdoc/>
	public IEnumerator<Inline> GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
