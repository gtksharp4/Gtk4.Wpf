using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base of <see cref="ListBox"/>, <see cref="ComboBox"/> and <see cref="TabControl"/>: keeps the selected item
/// in step with the current item of the items' <see cref="ICollectionView"/>.
/// </summary>
/// <remarks>
/// Each derived control still owns its <c>SelectedItem</c>/<c>SelectedIndex</c> properties and
/// <c>SelectionChanged</c> event; the shared base is limited to the view synchronisation.
/// </remarks>
public abstract class Selector : ItemsControl
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
	/// <see langword="null"/> synchronizes only when <see cref="ItemsControl.ItemsSource"/> is itself an
	/// <see cref="ICollectionView"/>.
	/// </summary>
	public bool? IsSynchronizedWithCurrentItem
	{
		get => (bool?)GetValue(IsSynchronizedWithCurrentItemProperty);
		set => SetValue(IsSynchronizedWithCurrentItemProperty, value);
	}

	bool IsSynchronizing => _view != null && (IsSynchronizedWithCurrentItem ?? ItemsSource is ICollectionView);

	/// <summary>The SelectedItem property of the derived control.</summary>
	internal abstract DependencyProperty SelectedItemDependencyProperty { get; }

	internal override void OnCollectionViewChanged(ICollectionView? view)
	{
		if (_view != null) _view.CurrentChanged -= OnViewCurrentChanged;
		_view = view;
		if (_view != null) _view.CurrentChanged += OnViewCurrentChanged;
		Resynchronize();
	}

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
		try { SetValue(SelectedItemDependencyProperty, item); }
		finally { _syncing = false; }
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (_syncing || !IsSynchronizing || e.Property != SelectedItemDependencyProperty) return;
		_syncing = true;
		try { _view!.MoveCurrentTo(e.NewValue); }
		finally { _syncing = false; }
	}
}
