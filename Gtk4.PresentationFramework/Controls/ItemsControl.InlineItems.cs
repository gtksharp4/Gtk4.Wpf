namespace System.Windows.Controls;

public partial class ItemsControl
{
	ItemCollection? _items;

	/// <summary>Called after an inline edit of <see cref="Items"/>.</summary>
	internal void OnInlineItemsEdited()
	{
		if (RebuildsOnInlineItemsChange) RebuildItems();
		else ExistingAutomationPeer?.NotifyChildrenChanged();
	}

	/// <summary>Whether inline <see cref="Items"/> edits rebuild the control (only ComboBox needs it).</summary>
	internal virtual bool RebuildsOnInlineItemsChange => false;

	/// <summary>Returns whether <paramref name="item"/> is its own container (WPF: <c>item is UIElement</c>).</summary>
	protected virtual bool IsItemItsOwnContainerOverride(object? item) => item is UIElement;

	/// <summary>Creates the container for an item (WPF default: a <see cref="ContentPresenter"/>).</summary>
	protected virtual DependencyObject GetContainerForItemOverride() => new ContentPresenter();

	/// <summary>
	/// Whether prefix text search is enabled. Store-only for GTK drop-down prefix search; ComboBox uses it
	/// (with <see cref="IsTextSearchCaseSensitive"/>) for exact Text-to-item matching.
	/// </summary>
	public static readonly DependencyProperty IsTextSearchEnabledProperty =
		DependencyProperty.Register(nameof(IsTextSearchEnabled), typeof(bool), typeof(ItemsControl),
			new FrameworkPropertyMetadata(false));

	/// <summary>Gets or sets whether text search is enabled.</summary>
	public bool IsTextSearchEnabled
	{
		get => (bool)GetValue(IsTextSearchEnabledProperty)!;
		set => SetValue(IsTextSearchEnabledProperty, value);
	}

	/// <summary>Whether text matching (e.g. <c>ComboBox.Text</c>) is case-sensitive.</summary>
	public static readonly DependencyProperty IsTextSearchCaseSensitiveProperty =
		DependencyProperty.Register(nameof(IsTextSearchCaseSensitive), typeof(bool), typeof(ItemsControl),
			new FrameworkPropertyMetadata(false));

	/// <summary>Gets or sets whether text matching is case-sensitive.</summary>
	public bool IsTextSearchCaseSensitive
	{
		get => (bool)GetValue(IsTextSearchCaseSensitiveProperty)!;
		set => SetValue(IsTextSearchCaseSensitiveProperty, value);
	}
}
