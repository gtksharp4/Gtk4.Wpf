using System;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Base class for resource keys that carry an owning assembly. Mirrors <c>ResourceKey</c>.</summary>
public abstract class ResourceKey : MarkupExtension
{
	/// <summary>The assembly that defines the resource.</summary>
	public abstract Assembly Assembly { get; }

	/// <summary>Returns this key.</summary>
	public override object? ProvideValue(IServiceProvider? serviceProvider) => this;
}
