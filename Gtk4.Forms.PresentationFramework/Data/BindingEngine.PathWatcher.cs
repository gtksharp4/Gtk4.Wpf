using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>
/// Per-segment INPC watcher that covers every node of a dot-separated path.
/// When any segment fires <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// for its own name, <see cref="Changed"/> is raised and the watcher re-subscribes
/// from that segment forward with the new intermediate values.
/// </summary>
/// <remarks>
/// Used internally by <see cref="DataTrigger"/> and <see cref="MultiDataTrigger"/>
/// to support nested binding paths (e.g. <c>Customer.Name</c>).
/// Dispose via <see cref="Dispose"/> to unsubscribe all INPC listeners.
/// </remarks>
internal sealed class PathWatcher : IDisposable
{
	readonly string[] _segments;
	readonly object? _root;

	// One subscription slot per intermediate segment (not the leaf — leaf fires via its parent).
	// _subs[i] is the subscription on the object at segment i-1 (or on root for i==0).
	readonly List<SegmentSub> _subs = new List<SegmentSub>();

	bool _disposed;

	/// <summary>Raised when any segment along the path fires PropertyChanged.</summary>
	internal event Action? Changed;

	/// <summary>
	/// Creates a watcher rooted at <paramref name="root"/> for a dot-separated
	/// <paramref name="path"/>. Subscribes immediately; <see cref="Changed"/> will
	/// fire when any node's relevant property changes.
	/// </summary>
	internal PathWatcher(object? root, string? path)
	{
		_root = root;
		_segments = string.IsNullOrEmpty(path)
			? Array.Empty<string>()
			: path!.Split('.');
		Subscribe();
	}

	/// <summary>
	/// Reads the current value at the end of the path starting from
	/// <paramref name="root"/>. Returns <c>null</c> when any node is null
	/// or the property is not found.
	/// </summary>
	internal static object? Evaluate(object? root, string? path)
		=> BindingEngine.ResolvePath(root, path);

	/// <summary>Reads the current path value from this watcher's root.</summary>
	internal object? CurrentValue => Evaluate(_root, string.Join(".", _segments));

	void Subscribe()
	{
		UnsubscribeAll();
		if (_segments.Length == 0) return;

		// Walk each prefix of the path and subscribe to its INPC if available.
		// segment[0] is a property on _root; we subscribe _root for segment[0].
		// segment[1] is a property on obj[0]; we subscribe obj[0] for segment[1], etc.
		var current = _root;
		for (int i = 0; i < _segments.Length; i++)
		{
			if (current == null) break;

			if (current is INotifyPropertyChanged inpc)
			{
				var capturedIdx = i;
				var capturedSegment = _segments[i];
				var sub = new SegmentSub(inpc, capturedSegment, capturedIdx, this);
				_subs.Add(sub);
			}

			// Advance to next object; stop if not found.
			var prop = current.GetType().GetProperty(
				_segments[i], BindingFlags.Public | BindingFlags.Instance);
			if (prop == null) break;
			current = prop.GetValue(current);
		}
	}

	void UnsubscribeAll()
	{
		foreach (var sub in _subs)
			sub.Detach();
		_subs.Clear();
	}

	void OnSegmentChanged(int segmentIndex)
	{
		// Re-subscribe from the changed segment forward (re-reads intermediate objects).
		ResubscribeFrom(segmentIndex);
		Changed?.Invoke();
	}

	void ResubscribeFrom(int fromIdx)
	{
		// Detach all subscriptions at fromIdx and beyond.
		for (int i = _subs.Count - 1; i >= 0; i--)
		{
			if (_subs[i].SegmentIndex >= fromIdx)
			{
				_subs[i].Detach();
				_subs.RemoveAt(i);
			}
		}

		// Re-walk from fromIdx.
		var current = _root;
		for (int i = 0; i < fromIdx; i++)
		{
			if (current == null) return;
			var prop = current.GetType().GetProperty(
				_segments[i], BindingFlags.Public | BindingFlags.Instance);
			if (prop == null) return;
			current = prop.GetValue(current);
		}

		for (int i = fromIdx; i < _segments.Length; i++)
		{
			if (current == null) break;

			if (current is INotifyPropertyChanged inpc)
			{
				// Only add if not already subscribed for this index.
				bool already = false;
				foreach (var s in _subs)
				{
					if (s.SegmentIndex == i) { already = true; break; }
				}
				if (!already)
				{
					var capturedIdx = i;
					var capturedSegment = _segments[i];
					_subs.Add(new SegmentSub(inpc, capturedSegment, capturedIdx, this));
				}
			}

			var p = current.GetType().GetProperty(
				_segments[i], BindingFlags.Public | BindingFlags.Instance);
			if (p == null) break;
			current = p.GetValue(current);
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		UnsubscribeAll();
		Changed = null;
	}

	// ── Inner subscription ───────────────────────────────────────────────────

	sealed class SegmentSub
	{
		readonly INotifyPropertyChanged _source;
		readonly string _propertyName;
		readonly PathWatcher _owner;
		readonly PropertyChangedEventHandler _handler;

		internal int SegmentIndex { get; }

		internal SegmentSub(
			INotifyPropertyChanged source,
			string propertyName,
			int segmentIndex,
			PathWatcher owner)
		{
			_source = source;
			_propertyName = propertyName;
			SegmentIndex = segmentIndex;
			_owner = owner;
			_handler = OnChanged;
			_source.PropertyChanged += _handler;
		}

		void OnChanged(object? sender, PropertyChangedEventArgs e)
		{
			// Fire when the event is for our segment's property or is a "all changed" broadcast.
			if (e.PropertyName != null && e.PropertyName != _propertyName) return;
			_owner.OnSegmentChanged(SegmentIndex);
		}

		internal void Detach() => _source.PropertyChanged -= _handler;
	}
}
