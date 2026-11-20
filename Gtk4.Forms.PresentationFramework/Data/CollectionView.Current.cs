using System.ComponentModel;

namespace System.Windows.Data;

public partial class CollectionView
{
	int _currentPosition;

	/// <inheritdoc/>
	public virtual object? CurrentItem =>
		_currentPosition >= 0 && _currentPosition < _view.Count ? _view[_currentPosition] : null;

	/// <inheritdoc/>
	public virtual int CurrentPosition => _currentPosition;

	/// <inheritdoc/>
	public virtual bool IsCurrentAfterLast => _currentPosition >= _view.Count;

	/// <inheritdoc/>
	public virtual bool IsCurrentBeforeFirst => _currentPosition < 0;

	/// <inheritdoc/>
	public virtual event CurrentChangingEventHandler? CurrentChanging;

	/// <inheritdoc/>
	public virtual event EventHandler? CurrentChanged;

	bool IsCurrentInView => _currentPosition >= 0 && _currentPosition < _view.Count;

	/// <inheritdoc/>
	public virtual bool MoveCurrentTo(object? item) => MoveCurrentToPosition(item == null ? -1 : _view.IndexOf(item));

	/// <inheritdoc/>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="position"/> is below -1 or above the count.
	/// </exception>
	public virtual bool MoveCurrentToPosition(int position)
	{
		if (position < -1 || position > _view.Count) throw new ArgumentOutOfRangeException(nameof(position));
		if (position != _currentPosition && TryChangeCurrent(position))
			RaiseCurrentChanged(true);
		return IsCurrentInView;
	}

	/// <inheritdoc/>
	public virtual bool MoveCurrentToFirst() => MoveCurrentToPosition(0);

	/// <inheritdoc/>
	public virtual bool MoveCurrentToLast() => MoveCurrentToPosition(_view.Count - 1);

	/// <inheritdoc/>
	public virtual bool MoveCurrentToNext() =>
		_currentPosition < _view.Count ? MoveCurrentToPosition(_currentPosition + 1) : IsCurrentInView;

	/// <inheritdoc/>
	public virtual bool MoveCurrentToPrevious() =>
		_currentPosition >= 0 ? MoveCurrentToPosition(_currentPosition - 1) : IsCurrentInView;

	bool TryChangeCurrent(int position)
	{
		var args = new CurrentChangingEventArgs();
		CurrentChanging?.Invoke(this, args);
		if (args.Cancel) return false;
		_currentPosition = position;
		return true;
	}

	void RaiseCurrentChanged(bool positionChanged)
	{
		CurrentChanged?.Invoke(this, EventArgs.Empty);
		OnPropertyChanged(nameof(CurrentItem));
		if (positionChanged) OnPropertyChanged(nameof(CurrentPosition));
		OnPropertyChanged(nameof(IsCurrentAfterLast));
		OnPropertyChanged(nameof(IsCurrentBeforeFirst));
	}
}
