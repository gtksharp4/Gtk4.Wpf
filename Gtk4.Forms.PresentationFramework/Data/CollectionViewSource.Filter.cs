using System.ComponentModel;

namespace System.Windows.Data;

public partial class CollectionViewSource
{
	FilterEventHandler? _filterHandlers;
	Predicate<object>? _installedFilter;

	/// <summary>
	/// Raised once per item to accept or reject it. While there is a handler the view's filter raises it and
	/// the final <see cref="FilterEventArgs.Accepted"/> wins; with none the filter is cleared.
	/// </summary>
	/// <exception cref="InvalidOperationException">The view cannot filter.</exception>
	public event FilterEventHandler Filter
	{
		add
		{
			_filterHandlers += value;
			if (_view != null) InstallFilter(_view);
		}
		remove
		{
			_filterHandlers -= value;
			if (_view != null) InstallFilter(_view);
		}
	}

	void InstallFilter(ICollectionView view)
	{
		if (_filterHandlers != null)
		{
			if (!view.CanFilter) throw new InvalidOperationException("The view does not support filtering.");
			_installedFilter ??= Accept;
			if (view.Filter != _installedFilter) view.Filter = _installedFilter;
			else view.Refresh();
		}
		else if (_installedFilter != null && view.Filter == _installedFilter)
		{
			view.Filter = null;
		}
	}

	bool Accept(object item)
	{
		var args = new FilterEventArgs(item);
		_filterHandlers?.Invoke(this, args);
		return args.Accepted;
	}
}
