using System.Collections.Generic;

namespace System.Windows;

public abstract partial class UIElement
{
	// A Freezable stored in a property (Background brush, RenderTransform) mutates in place when
	// animated; the owner re-runs the property's change callback so it repaints.
	Dictionary<DependencyProperty, (Freezable Value, EventHandler Handler)>? _watchedFreezables;

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.OldValue is Freezable || e.NewValue is Freezable)
			WatchFreezable(e.Property, e.NewValue as Freezable);
	}

	void WatchFreezable(DependencyProperty dp, Freezable? value)
	{
		if (_watchedFreezables != null && _watchedFreezables.TryGetValue(dp, out var old))
		{
			old.Value.Changed -= old.Handler;
			_watchedFreezables.Remove(dp);
		}
		if (value == null || value.IsFrozen) return;
		EventHandler handler = (s, a) => RaiseFreezableChanged(dp, value);
		value.Changed += handler;
		(_watchedFreezables ??= new Dictionary<DependencyProperty, (Freezable, EventHandler)>())[dp] = (value, handler);
	}

	void RaiseFreezableChanged(DependencyProperty dp, Freezable value) =>
		dp.GetMetadata(GetType()).PropertyChangedCallback?.Invoke(
			this, new DependencyPropertyChangedEventArgs(dp, value, value));
}
