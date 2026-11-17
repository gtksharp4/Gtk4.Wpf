using System;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{DynamicResource key}</c>.
/// Phase-1 stub: at runtime, resolves the key once from the target element's resource
/// chain (same as <see cref="StaticResourceExtension"/>).  Live-update re-resolution on
/// resource-dictionary changes is a future phase.
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
	/// Phase-1: resolves once at call time (no live change subscription yet).
	/// </remarks>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (ResourceKey == null)
			throw new InvalidOperationException("DynamicResourceExtension: ResourceKey is null.");

		if (serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget pvt
		    && pvt.TargetObject is FrameworkElement fe)
		{
			return fe.FindResource(ResourceKey);
		}

		if (Application.Current?.Resources.TryGetValue(ResourceKey, out var appVal) == true)
			return appVal;

		throw new InvalidOperationException(
			$"DynamicResource: resource with key '{ResourceKey}' not found.");
	}
}
