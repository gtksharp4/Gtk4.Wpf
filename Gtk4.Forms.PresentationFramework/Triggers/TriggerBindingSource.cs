using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// The live value behind the <see cref="Binding"/> of a data trigger or condition: resolves the root (Source,
/// ElementName, RelativeSource or the DataContext), watches the path along it (CLR, dependency and attached
/// properties), applies the converter and reports every change. It re-resolves when the data context changes or
/// the tree around the target changes, as an ElementName or RelativeSource may become resolvable only then.
/// </summary>
internal sealed class TriggerBindingSource : BindingEngine.IContextListener, IDisposable
{
	readonly Binding _binding;
	readonly FrameworkElement _target;
	readonly Action _changed;
	readonly PropertyChangedCallback _dataContextHook;
	PathWatcher? _watcher;
	object? _root;
	bool _disposed;

	internal TriggerBindingSource(Binding binding, FrameworkElement target, Action changed)
	{
		_binding = binding;
		_target = target;
		_changed = changed;
		_dataContextHook = (d, e) => Resubscribe();
		_target.AddChangeHook(FrameworkElement.DataContextProperty, _dataContextHook);
		if (_binding.ElementName != null || _binding.RelativeSource != null)
			BindingEngine.RegisterContextListener(this);
		Watch();
	}

	/// <summary>The bound value after the path and the converter.</summary>
	internal object? Value
	{
		get
		{
			var raw = BindingEngine.ResolvePath(_root, _binding.PathString);
			if (_binding.Converter == null) return raw;
			var culture = BindingEngine.ResolveCulture(_binding.ConverterCulture);
			var converted = _binding.Converter.Convert(raw, typeof(object), _binding.ConverterParameter, culture);
			return ReferenceEquals(converted, Binding.DoNothing) || ReferenceEquals(converted, DependencyProperty.UnsetValue)
				? null
				: converted;
		}
	}

	object? ResolveRoot()
	{
		if (_binding.Source != null) return _binding.Source;
		if (_binding.ElementName != null) return BindingTarget.FindName(_target, _binding.ElementName);
		if (_binding.RelativeSource != null) return BindingTarget.ResolveRelative(_target, _binding.RelativeSource);
		return _target.DataContext;
	}

	void Watch()
	{
		_watcher?.Dispose();
		_root = ResolveRoot();
		_watcher = new PathWatcher(_root, PropertyPath.ResolveOwner(_root, _binding.PathString));
		_watcher.Changed += _changed;
	}

	void Resubscribe()
	{
		if (_disposed) return;
		Watch();
		_changed();
	}

	string? BindingEngine.IContextListener.ElementName => _binding.ElementName;

	bool BindingEngine.IContextListener.IsUnder(DependencyObject ancestor) =>
		BindingTarget.IsUnder(_target, ancestor);

	void BindingEngine.IContextListener.ReResolve()
	{
		if (!ReferenceEquals(ResolveRoot(), _root)) Resubscribe();
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		_target.RemoveChangeHook(FrameworkElement.DataContextProperty, _dataContextHook);
		BindingEngine.UnregisterContextListener(this);
		_watcher?.Dispose();
		_watcher = null;
	}
}
