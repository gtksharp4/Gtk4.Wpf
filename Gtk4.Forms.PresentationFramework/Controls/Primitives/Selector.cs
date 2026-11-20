using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base of <see cref="ListBox"/>, <see cref="ComboBox"/> and <see cref="TabControl"/>: keeps the selected item
/// in step with the current item of the items' <see cref="ICollectionView"/>.
/// </summary>
/// <remarks>
/// Each derived control still owns its <c>SelectedItem</c>/<c>SelectedIndex</c> properties;
/// <c>SelectionChanged</c>/<c>SelectionChangedEvent</c> are declared on <see cref="Selector"/> itself.
/// </remarks>
public abstract partial class Selector : ItemsControl
{
	/// <summary>Identifies the <see cref="IsSynchronizedWithCurrentItem"/> property.</summary>
	public static readonly DependencyProperty IsSynchronizedWithCurrentItemProperty =
		DependencyProperty.Register(
			nameof(IsSynchronizedWithCurrentItem), typeof(bool?), typeof(Selector),
			new FrameworkPropertyMetadata(null, OnIsSynchronizedChanged));

	static void OnIsSynchronizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Selector)d).Resynchronize();

	ICollectionView? _view;
	bool _syncing;

	/// <summary>
	/// Gets or sets whether the selection follows the current item of the items' view and vice versa.
	/// <see langword="null"/> synchronizes only when the view is not the default view of a plain
	/// <see cref="ItemsControl.ItemsSource"/> (for a <see cref="ListBox"/> also only in single selection mode).
	/// </summary>
	public bool? IsSynchronizedWithCurrentItem
	{
		get => (bool?)GetValue(IsSynchronizedWithCurrentItemProperty);
		set => SetValue(IsSynchronizedWithCurrentItemProperty, value);
	}

	bool IsSynchronizing => _view != null && SelectedItemDependencyProperty != null
		&& (IsSynchronizedWithCurrentItem
			?? (ImplicitSynchronization && _view is not CollectionView { IsDefaultView: true }));

	/// <summary>The SelectedItem property of the derived control; null means it does not synchronize.</summary>
	internal virtual DependencyProperty? SelectedItemDependencyProperty => null;

	/// <summary>Whether an unset <see cref="IsSynchronizedWithCurrentItem"/> synchronizes a non-default view.</summary>
	internal virtual bool ImplicitSynchronization => true;

	internal override void OnItemsRebuildCompleted() => Resynchronize();

	internal override void AttachCollectionView(ICollectionView? view)
	{
		if (_view != null) _view.CurrentChanged -= OnViewCurrentChanged;
		_view = view;
		if (_view != null) _view.CurrentChanged += OnViewCurrentChanged;
	}

	internal override void OnCollectionViewChanged(ICollectionView? view) => Resynchronize();

	void OnViewCurrentChanged(object? sender, EventArgs e)
	{
		if (_syncing || !IsSynchronizing) return;
		Select(_view!.CurrentItem);
	}

	void Resynchronize()
	{
		if (IsSynchronizing) Select(_view!.CurrentItem);
	}

	void Select(object? item)
	{
		_syncing = true;
		try { SetValue(SelectedItemDependencyProperty!, item); }
		finally { _syncing = false; }
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		// A rebuild clears the selection without the user choosing anything: the view keeps its current item.
		if (_syncing || IsRebuildingItems || !IsSynchronizing || e.Property != SelectedItemDependencyProperty) return;
		_syncing = true;
		try { _view!.MoveCurrentTo(e.NewValue); }
		finally { _syncing = false; }
	}
}
