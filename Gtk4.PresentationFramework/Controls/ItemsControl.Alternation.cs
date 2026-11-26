namespace System.Windows.Controls;

/// <summary>
/// Alternation support for <see cref="ItemsControl"/>:
/// <see cref="AlternationCountProperty"/> and the read-only attached
/// <see cref="AlternationIndexProperty"/> / <see cref="GetAlternationIndex"/>.
/// </summary>
public partial class ItemsControl
{
	// ── AlternationCount DP ──────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <c>AlternationCount</c> dependency property.
	/// Default value is 0 (alternation disabled).
	/// </summary>
	public static readonly DependencyProperty AlternationCountProperty =
		DependencyProperty.Register(
			nameof(AlternationCount),
			typeof(int),
			typeof(ItemsControl),
			new PropertyMetadata(0, OnAlternationCountChanged));

	static void OnAlternationCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ItemsControl)d).ReassignAlternationIndices();
	}

	/// <summary>
	/// Gets or sets the number of alternating item containers.
	/// 0 (default) disables alternation; all containers get index 0.
	/// A positive value N cycles containers through indices 0..N-1.
	/// </summary>
	public int AlternationCount
	{
		get => (int)GetValue(AlternationCountProperty)!;
		set => SetValue(AlternationCountProperty, value);
	}

	// ── AlternationIndex (read-only attached DP) ─────────────────────────────

	static readonly DependencyPropertyKey _alternationIndexKey =
		DependencyProperty.RegisterReadOnly(
			"AlternationIndex",
			typeof(int),
			typeof(ItemsControl),
			new PropertyMetadata(0));

	/// <summary>
	/// Identifies the <c>ItemsControl.AlternationIndex</c> read-only attached property.
	/// Set by the owning <see cref="ItemsControl"/> on each generated container.
	/// </summary>
	public static readonly DependencyProperty AlternationIndexProperty =
		_alternationIndexKey.DependencyProperty;

	/// <summary>
	/// Returns the alternation index assigned to <paramref name="element"/> by its
	/// parent <see cref="ItemsControl"/>.
	/// Returns 0 when the property has not been set (alternation disabled).
	/// </summary>
	/// <param name="element">The container element to query.</param>
	public static int GetAlternationIndex(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (int)element.GetValue(AlternationIndexProperty)!;
	}

	// ── Pure computation ─────────────────────────────────────────────────────

	/// <summary>
	/// Computes the alternation index for item at <paramref name="itemIndex"/> given
	/// <paramref name="count"/> alternating buckets.
	/// Returns 0 when <paramref name="count"/> is 0 or 1 (WPF semantics: disabled/trivial).
	/// </summary>
	internal static int ComputeAlternationIndex(int itemIndex, int count) =>
		count > 1 ? itemIndex % count : 0;

	// ── Assignment ───────────────────────────────────────────────────────────

	void AssignAlternationIndex(DependencyObject container, int itemIndex)
	{
		container.SetValue(_alternationIndexKey, ComputeAlternationIndex(itemIndex, AlternationCount));
	}

	void ReassignAlternationIndices()
	{
		for (int i = 0; i < _rows.Count; i++)
			AssignAlternationIndex(_rows[i].Container, i);
	}

	// ── Hook into PopulateRows ────────────────────────────────────────────────

	/// <summary>
	/// Called by <see cref="PopulateRows"/> after each container is added to the row list.
	/// Assigns the <see cref="AlternationIndexProperty"/> for the container at index
	/// <paramref name="itemIndex"/>.
	/// </summary>
	void OnContainerAdded(DependencyObject container, int itemIndex)
	{
		AssignAlternationIndex(container, itemIndex);
	}
}
