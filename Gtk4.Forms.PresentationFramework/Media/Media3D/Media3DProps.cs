namespace System.Windows.Media.Media3D;

/// <summary>Helpers shared by the animatable 3D classes.</summary>
static class Media3DProps
{
	/// <summary>
	/// Returns the collection of <paramref name="dp"/>; a frozen shared default is replaced by a private
	/// collection the first time an unfrozen owner is asked for it, as WPF does.
	/// </summary>
	internal static T Collection<T>(Freezable owner, DependencyProperty dp, T empty) where T : Freezable, new()
	{
		var value = (T)owner.GetValue(dp)!;
		if (ReferenceEquals(value, empty) && !owner.IsFrozen)
		{
			value = new T();
			owner.SetValue(dp, value);
		}
		return value;
	}

	/// <summary>Reads a collection without ever materialising it (used by the renderer).</summary>
	internal static T Peek<T>(DependencyObject owner, DependencyProperty dp) where T : class =>
		(T)owner.GetValue(dp)!;

	/// <summary>Registers a DP whose default value is a frozen shared instance or a plain value.</summary>
	internal static DependencyProperty Register(string name, Type type, Type owner, object? defaultValue) =>
		DependencyProperty.Register(name, type, owner, new PropertyMetadata(defaultValue));

	/// <summary>Freezes every freezable among <paramref name="values"/>.</summary>
	internal static void FreezeAll(params object?[] values)
	{
		foreach (var v in values)
			(v as Freezable)?.Freeze();
	}
}

/// <summary>Forwards <c>Changed</c> of freezable DP values to their owner.</summary>
sealed class ChildLinks
{
	readonly FreezableChildWatcher _watcher;

	internal ChildLinks(Action raise) => _watcher = new FreezableChildWatcher(raise);

	internal void Track(DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			_watcher.Set(e.Property, e.NewValue);
	}
}
