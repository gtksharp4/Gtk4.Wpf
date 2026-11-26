namespace System.Windows.Controls;

/// <summary>
/// Defines the visual representation of a data item. The
/// <see cref="ItemFactory"/> receives the data item and returns a
/// <see cref="UIElement"/> that represents it.
/// <para>
/// For simple cases set <see cref="FrameworkTemplate.Factory"/> (item-independent);
/// for item-aware templates set <see cref="ItemFactory"/>.
/// XAML <c>&lt;DataTemplate&gt;</c> parsing is a later compiler round.
/// </para>
/// </summary>
public partial class DataTemplate : FrameworkTemplate
{
	/// <summary>The data type this template applies to (store-only; no implicit selection).</summary>
	public object? DataType { get; set; }

	/// <summary>
	/// A factory that creates a visual for a specific data item.
	/// Takes precedence over <see cref="FrameworkTemplate.Factory"/> when set.
	/// </summary>
	public Func<object?, UIElement>? ItemFactory { get; set; }

	/// <summary>
	/// Creates a visual for <paramref name="dataItem"/>.
	/// Uses <see cref="ItemFactory"/> if set, otherwise
	/// <see cref="FrameworkTemplate.Factory"/>.
	/// Returns <c>null</c> when neither factory is set.
	/// </summary>
	public UIElement? CreateVisualTreeForItem(object? dataItem)
	{
		if (ItemFactory != null) return ItemFactory(dataItem);
		return Factory?.Invoke();
	}

	/// <inheritdoc/>
	public override bool HasContent => ItemFactory != null || Factory != null;
}