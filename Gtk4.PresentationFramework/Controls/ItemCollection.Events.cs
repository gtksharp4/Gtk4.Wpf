using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls;

public sealed partial class ItemCollection
{
	// The events are re-raised from this collection, from either the inline view or the source view.
	NotifyCollectionChangedEventHandler? _changed;
	PropertyChangedEventHandler? _propertyChanged;
	CurrentChangingEventHandler? _currentChanging;
	EventHandler? _currentChanged;

	/// <inheritdoc/>
	public override event NotifyCollectionChangedEventHandler? CollectionChanged
	{
		add => _changed += value;
		remove => _changed -= value;
	}

	/// <inheritdoc/>
	protected override event PropertyChangedEventHandler? PropertyChanged
	{
		add => _propertyChanged += value;
		remove => _propertyChanged -= value;
	}

	/// <inheritdoc/>
	public override event CurrentChangingEventHandler? CurrentChanging
	{
		add => _currentChanging += value;
		remove => _currentChanging -= value;
	}

	/// <inheritdoc/>
	public override event EventHandler? CurrentChanged
	{
		add => _currentChanged += value;
		remove => _currentChanged -= value;
	}

	void AttachSource()
	{
		if (_sourceView == null) return;
		_sourceView.CollectionChanged += OnSourceCollectionChanged;
		_sourceView.CurrentChanging += OnSourceCurrentChanging;
		_sourceView.CurrentChanged += OnSourceCurrentChanged;
		if (_sourceView is INotifyPropertyChanged npc) npc.PropertyChanged += OnSourcePropertyChanged;
	}

	void DetachSource()
	{
		if (_sourceView == null) return;
		_sourceView.CollectionChanged -= OnSourceCollectionChanged;
		_sourceView.CurrentChanging -= OnSourceCurrentChanging;
		_sourceView.CurrentChanged -= OnSourceCurrentChanged;
		if (_sourceView is INotifyPropertyChanged npc) npc.PropertyChanged -= OnSourcePropertyChanged;
	}

	void OnSourceCollectionChanged(object? s, NotifyCollectionChangedEventArgs e) => _changed?.Invoke(this, e);

	void OnSourceCurrentChanging(object? s, CurrentChangingEventArgs e) => _currentChanging?.Invoke(this, e);

	void OnSourceCurrentChanged(object? s, EventArgs e) => _currentChanged?.Invoke(this, e);

	void OnSourcePropertyChanged(object? s, PropertyChangedEventArgs e) => _propertyChanged?.Invoke(this, e);
}
