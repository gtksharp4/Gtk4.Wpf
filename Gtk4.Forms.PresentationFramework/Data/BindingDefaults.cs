using System;

namespace System.Windows.Data;

/// <summary>
/// Resolves <see cref="BindingMode.Default"/> and <see cref="UpdateSourceTrigger.Default"/> from the
/// target property's metadata, as WPF does.
/// </summary>
internal static class BindingDefaults
{
	internal static (BindingMode Mode, UpdateSourceTrigger Trigger) Resolve(
		BindingMode mode,
		UpdateSourceTrigger trigger,
		DependencyProperty dp,
		Type targetType)
	{
		var meta = dp.GetMetadata(targetType) as FrameworkPropertyMetadata;
		if (mode == BindingMode.Default)
			mode = meta != null && meta.BindsTwoWayByDefault ? BindingMode.TwoWay : BindingMode.OneWay;
		if (trigger == UpdateSourceTrigger.Default)
			trigger = meta != null ? meta.DefaultUpdateSourceTrigger : UpdateSourceTrigger.PropertyChanged;
		return (mode, trigger);
	}
}
