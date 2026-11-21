using System.Collections.ObjectModel;

namespace System.Windows.Documents;

/// <summary>
/// A list of text elements owned by one parent. Adding an element sets its logical parent; adding one that already
/// has a parent throws <see cref="InvalidOperationException"/>, as in WPF.
/// </summary>
public abstract class TextElementCollection<TextElementType> : Collection<TextElementType>
	where TextElementType : TextElement
{
	readonly DependencyObject _owner;

	internal TextElementCollection(DependencyObject owner) => _owner = owner;

	/// <inheritdoc/>
	protected override void InsertItem(int index, TextElementType item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		ContentParenting.Adopt(_owner, item);
		base.InsertItem(index, item);
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, TextElementType item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		var old = this[index];
		if (ReferenceEquals(old, item)) return;
		ContentParenting.Adopt(_owner, item);
		ContentParenting.Release(_owner, old);
		base.SetItem(index, item);
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		ContentParenting.Release(_owner, this[index]);
		base.RemoveItem(index);
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		foreach (var item in this) ContentParenting.Release(_owner, item);
		base.ClearItems();
	}
}

/// <summary>The row groups of a <see cref="Table"/>.</summary>
public sealed class TableRowGroupCollection : TextElementCollection<TableRowGroup>
{
	internal TableRowGroupCollection(Table owner) : base(owner) { }
}

/// <summary>The rows of a <see cref="TableRowGroup"/>.</summary>
public sealed class TableRowCollection : TextElementCollection<TableRow>
{
	internal TableRowCollection(TableRowGroup owner) : base(owner) { }
}

/// <summary>The cells of a <see cref="TableRow"/>.</summary>
public sealed class TableCellCollection : TextElementCollection<TableCell>
{
	internal TableCellCollection(TableRow owner) : base(owner) { }
}

/// <summary>The columns of a <see cref="Table"/>.</summary>
public sealed class TableColumnCollection : Collection<TableColumn>
{
	readonly Table _owner;

	internal TableColumnCollection(Table owner) => _owner = owner;

	/// <inheritdoc/>
	protected override void InsertItem(int index, TableColumn item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		ContentParenting.Adopt(_owner, item);
		base.InsertItem(index, item);
	}

	/// <inheritdoc/>
	protected override void SetItem(int index, TableColumn item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		var old = this[index];
		if (ReferenceEquals(old, item)) return;
		ContentParenting.Adopt(_owner, item);
		ContentParenting.Release(_owner, old);
		base.SetItem(index, item);
	}

	/// <inheritdoc/>
	protected override void RemoveItem(int index)
	{
		ContentParenting.Release(_owner, this[index]);
		base.RemoveItem(index);
	}

	/// <inheritdoc/>
	protected override void ClearItems()
	{
		foreach (var item in this) ContentParenting.Release(_owner, item);
		base.ClearItems();
	}
}
