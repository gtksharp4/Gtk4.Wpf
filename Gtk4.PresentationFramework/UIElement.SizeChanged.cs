using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// The size-changed chain (WPF's <c>ContextLayoutManager.AddToSizeChangedChain</c> /
/// <c>fireSizeChangedEvents</c>): <see cref="Arrange"/> only queues a
/// <see cref="SizeChangedInfo"/>; the queued notifications run after the outermost arrange pass
/// returns, so every handler observes final sizes and nothing re-enters layout mid-pass.
/// </summary>
/// <remarks>
/// As in WPF the chain is prepended, so notifications typically run top-down (a parent is
/// arranged after its children have been, and so is queued last — and fired first).
/// An element arranged twice in one pass keeps its first <see cref="SizeChangedInfo"/>, with the
/// changed axes merged.
/// </remarks>
public abstract partial class UIElement
{
	[ThreadStatic] static List<SizeChangedInfo>? _sizeChangedChain;
	[ThreadStatic] static int _arrangeDepth;

	SizeChangedInfo? _pendingSizeChangedInfo;

	/// <summary>
	/// <see langword="true"/> while the live GTK allocation is the sole writer of
	/// <see cref="RenderSize"/> (a loaded element with a mapped widget). Overridden by
	/// <see cref="FrameworkElement"/>.
	/// </summary>
	internal virtual bool IsRenderSizeOwnedByGtk => false;

	void QueueSizeChanged(Size previous)
	{
		if (!SizeChangeMath.Compute(previous, _renderSize, out bool widthChanged, out bool heightChanged))
			return;
		if (_pendingSizeChangedInfo != null)
		{
			_pendingSizeChangedInfo.Merge(widthChanged, heightChanged);
			return;
		}
		var info = new SizeChangedInfo(this, previous, widthChanged, heightChanged);
		_pendingSizeChangedInfo = info;
		(_sizeChangedChain ??= new List<SizeChangedInfo>()).Add(info);
	}

	static void FlushSizeChangedChain()
	{
		var chain = _sizeChangedChain;
		if (chain == null || chain.Count == 0)
			return;
		_sizeChangedChain = null;
		// Fire in prepend order (last queued first). Handlers that re-arrange start a new chain.
		for (int i = chain.Count - 1; i >= 0; i--)
		{
			var info = chain[i];
			info.Element._pendingSizeChangedInfo = null;
			info.Element.OnRenderSizeChanged(info);
		}
	}

	/// <summary>
	/// Stores <paramref name="size"/> as <see cref="RenderSize"/> and, when it differs on either
	/// axis, calls <see cref="OnRenderSizeChanged"/> immediately — the live-allocation path
	/// (<c>GtkAllocationSync</c>), which walks the tree top-down itself.
	/// </summary>
	internal void SetRenderSizeAndNotify(Size size)
	{
		var previous = _renderSize;
		_renderSize = size;
		if (SizeChangeMath.Compute(previous, size, out bool widthChanged, out bool heightChanged))
			OnRenderSizeChanged(new SizeChangedInfo(this, previous, widthChanged, heightChanged));
	}
}
