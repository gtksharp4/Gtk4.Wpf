using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

public abstract partial class BitmapSource
{
	static readonly DependencyProperty[] NoInputs = new DependencyProperty[0];

	Dictionary<DependencyProperty, object?>? _initialised;

	/// <summary>The properties that WPF coerces back to their initialised values after EndInit.</summary>
	internal virtual DependencyProperty[] InputProperties => NoInputs;

	/// <summary>Returns the initialised value of <paramref name="dp"/> outside BeginInit/EndInit.</summary>
	internal static object? CoerceInput(DependencyObject d, DependencyProperty? dp, object? baseValue) =>
		d is BitmapSource bitmap && bitmap._initialised != null && !bitmap.IsInitializing
			&& dp != null && bitmap._initialised.TryGetValue(dp, out var value) ? value : baseValue;

	internal void LockInputs()
	{
		var snapshot = new Dictionary<DependencyProperty, object?>();
		foreach (var dp in InputProperties)
			snapshot[dp] = GetValue(dp);
		_initialised = snapshot;
	}

	internal void UnlockInputs()
	{
		_initialised = null;
		foreach (var dp in InputProperties)
			CoerceValue(dp);
	}

	internal static bool CanFreezeInput(Freezable? input) => input == null || input.IsFrozen || input.CanFreeze;

	internal static void FreezeValue(Freezable? input) => input?.Freeze();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		if (_initialised != null)
			((BitmapSource)destination).LockInputs();
	}
}
