using System;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{DynamicResource key}</c>.
/// When the target is a <see cref="FrameworkElement"/> and <see cref="IProvideValueTarget"/>
/// exposes a <see cref="DependencyProperty"/>, routes through
/// <see cref="FrameworkElement.SetResourceReference"/> so the DP tracks live resource
/// changes.  For other targets, resolves once at call time.
/// </summary>
public class DynamicResourceExtension : MarkupExtension
{
	/// <summary>The resource key to resolve.</summary>
	public object? ResourceKey { get; set; }

	/// <summary>Creates an empty extension (key set via <see cref="ResourceKey"/>).</summary>
	public DynamicResourceExtension() { }

	/// <summary>Creates the extension with the given positional key.</summary>
	public DynamicResourceExtension(object resourceKey) => ResourceKey = resourceKey;

	/// <inheritdoc/>
	/// <remarks>
	/// When <paramref name="serviceProvider"/> exposes a <see cref="IProvideValueTarget"/>
	/// whose <c>TargetObject</c> is a <see cref="FrameworkElement"/> and
	/// <c>TargetProperty</c> is a <see cref="DependencyProperty"/>, calls
	/// <see cref="FrameworkElement.SetResourceReference"/> to establish a live link and
	/// returns <c>null</c> (the DP value is set directly by the reference mechanism).
	/// Otherwise resolves once from the application resources.
	/// </remarks>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (ResourceKey == null)
			throw new InvalidOperationException("DynamicResourceExtension: ResourceKey is null.");

		if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt
		    && pvt.TargetObject is FrameworkElement fe
		    && pvt.TargetProperty is DependencyProperty dp)
		{
			fe.SetResourceReference(dp, ResourceKey);
			// SetResourceReference pushes the value via SetDynResValue; return null so
			// the caller does not overwrite it with a local set.
			return null;
		}

		// No element+DP context: resolve once from application resources.
		if (Application.Current?.Resources.TryGetValue(ResourceKey, out var appVal) == true)
			return appVal;

		throw new InvalidOperationException(
			$"DynamicResource: resource with key '{ResourceKey}' not found.");
	}
}
