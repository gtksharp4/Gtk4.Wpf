using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>
/// Watches every node of a dotted path: <see cref="INotifyPropertyChanged"/> sources by property name, and
/// <see cref="DependencyObject"/> sources by the dependency property a step names (plain or attached). Any
/// change re-walks the path, re-subscribes along the new intermediate objects and raises <see cref="Changed"/>.
/// </summary>
internal sealed class PathWatcher : IDisposable
{
	readonly string? _path;
	readonly object? _root;
	readonly List<Action> _detach = new List<Action>();
	bool _disposed;

	/// <summary>Raised when any node along the path changes.</summary>
	internal event Action? Changed;

	internal PathWatcher(object? root, string? path)
	{
		_root = root;
		_path = path;
		Subscribe();
	}

	/// <summary>Reads the current path value from this watcher's root.</summary>
	internal object? CurrentValue => BindingEngine.ResolvePath(_root, _path);

	void Subscribe()
	{
		Detach();
		if (_disposed || string.IsNullOrEmpty(_path)) return;
		var current = _root;
		foreach (var step in BindingEngine.ParseSteps(_path!))
		{
			if (current == null || step.Name == null) return;
			Watch(current, step.Name);
			if (!BindingEngine.TryGetMember(current, step.Name, null, out current)) return;
		}
	}

	void Watch(object node, string step)
	{
		var name = BindingEngine.ClrNameOfOwnedStep(node, step) ?? step;
		if (node is INotifyPropertyChanged inpc)
		{
			PropertyChangedEventHandler handler = (s, e) =>
			{
				if (e.PropertyName == null || e.PropertyName.Length == 0 || e.PropertyName == name) OnNodeChanged();
			};
			inpc.PropertyChanged += handler;
			_detach.Add(() => inpc.PropertyChanged -= handler);
		}
		if (node is DependencyObject d)
		{
			var dp = name[0] == '(' ? BindingEngine.AttachedProperty(node, name) : BindingEngine.NamedProperty(node, name);
			if (dp == null) return;
			PropertyChangedCallback callback = (o, e) => OnNodeChanged();
			d.AddChangeHook(dp, callback);
			_detach.Add(() => d.RemoveChangeHook(dp, callback));
		}
	}

	void OnNodeChanged()
	{
		if (_disposed) return;
		Subscribe();
		Changed?.Invoke();
	}

	void Detach()
	{
		foreach (var undo in _detach) undo();
		_detach.Clear();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		Detach();
		Changed = null;
	}
}
