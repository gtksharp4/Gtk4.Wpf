using System.Windows.Threading;

namespace System.Windows.Documents;

public partial class FlowDocument
{
	bool _viewDirty, _viewPosted;

	/// <summary>Tells the hosting viewers to re-render and the paginator to repaginate (the content changed).</summary>
	internal void NotifyContentEdited()
	{
		InvalidateSymbols();
		RaiseViewChanged();
		ContentChanged?.Invoke();
	}

	/// <summary>
	/// Marks the view dirty and posts one callback per dispatcher turn that raises <see cref="ViewChanged"/>, so a
	/// burst of edits (or an inherited property change reaching every element) re-renders once.
	/// </summary>
	internal void RaiseViewChanged()
	{
		if (ViewChanged == null) return;
		_viewDirty = true;
		if (_viewPosted) return;
		_viewPosted = true;
		Dispatcher.BeginInvoke(() =>
		{
			_viewPosted = false;
			FlushViewChanged();
		}, DispatcherPriority.Render);
	}

	/// <summary>Raises a pending <see cref="ViewChanged"/> now.</summary>
	internal void FlushViewChanged()
	{
		if (!_viewDirty) return;
		_viewDirty = false;
		ViewChanged?.Invoke();
	}
}
