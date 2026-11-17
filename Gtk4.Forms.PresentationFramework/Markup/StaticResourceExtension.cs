using System;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{StaticResource key}</c>.
/// <see cref="ProvideValue"/> resolves the key by walking the target element's resource
/// chain (via <see cref="FrameworkElement.FindResource"/>) and then
/// <see cref="Application.Current"/>?.Resources.
/// </summary>
public class StaticResourceExtension : MarkupExtension
{
	/// <summary>The resource key to resolve.</summary>
	public object? ResourceKey { get; set; }

	/// <summary>Creates an empty extension (key set via <see cref="ResourceKey"/>).</summary>
	public StaticResourceExtension() { }

	/// <summary>Creates the extension with the given positional key.</summary>
	public StaticResourceExtension(object resourceKey) => ResourceKey = resourceKey;

	/// <inheritdoc/>
	/// <remarks>
	/// Resolution order (mirrors WPF):
	/// 1. Walk from the target element up through its logical-parent chain, checking
	///    <see cref="FrameworkElement.Resources"/> at each step.
	/// 2. <see cref="Application.Current"/>?.Resources.
	/// Throws <see cref="InvalidOperationException"/> when the key is not found.
	/// </remarks>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (ResourceKey == null)
			throw new InvalidOperationException("StaticResourceExtension: ResourceKey is null.");

		// Try the target element if the service provider exposes one.
		if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt
		    && pvt.TargetObject is FrameworkElement fe)
		{
			return fe.FindResource(ResourceKey);
		}

		// No element context — fall back to application resources only.
		if (Application.Current?.Resources.TryGetValue(ResourceKey, out var appVal) == true)
			return appVal;

		throw new InvalidOperationException(
			$"StaticResource: resource with key '{ResourceKey}' not found.");
	}
}
