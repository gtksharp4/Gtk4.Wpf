using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents
{
	/// <summary>
	/// An ordered collection of <see cref="Inline"/> elements that belong to a single
	/// <see cref="Controls.TextBlock"/> or <see cref="Span"/>.
	/// </summary>
	public sealed class InlineCollection : IEnumerable<Inline>
	{
		readonly List<Inline> _items = new List<Inline>();

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
			_items.Add(inline);
			Changed?.Invoke();
		}

		/// <summary>Removes an inline from the collection.</summary>
		public void Remove(Inline inline)
		{
			if (inline == null) throw new ArgumentNullException(nameof(inline));
			_items.Remove(inline);
			Changed?.Invoke();
		}

		/// <summary>Removes all inlines.</summary>
		public void Clear()
		{
			_items.Clear();
			Changed?.Invoke();
		}

		/// <inheritdoc/>
		public IEnumerator<Inline> GetEnumerator() => _items.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}
}
