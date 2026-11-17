using System;

namespace System.Windows.Markup;

// ── Service interfaces ───────────────────────────────────────────────────────

/// <summary>
/// Provides the target object and property for a markup extension that is being
/// evaluated at runtime (mirrors WPF's <c>IProvideValueTarget</c>).
/// </summary>
public interface IProvideValueTarget
{
	/// <summary>The object on which the markup extension is being set.</summary>
	object? TargetObject { get; }

	/// <summary>
	/// The property (a <see cref="DependencyProperty"/> or <see cref="System.Reflection.PropertyInfo"/>)
	/// on which the markup extension value will be placed.
	/// </summary>
	object? TargetProperty { get; }
}

/// <summary>
/// Resolves an XML-namespace-qualified type name to a <see cref="Type"/> at runtime.
/// </summary>
public interface IXamlTypeResolver
{
	/// <summary>Resolves <paramref name="qualifiedTypeName"/> to its CLR <see cref="Type"/>.</summary>
	Type Resolve(string qualifiedTypeName);
}

/// <summary>
/// Provides the base URI for a markup extension that needs to resolve relative URIs.
/// </summary>
public interface IUriContext
{
	/// <summary>The base URI for the current context.</summary>
	Uri? BaseUri { get; set; }
}

// ── MarkupExtension ──────────────────────────────────────────────────────────

/// <summary>
/// Abstract base class for all XAML markup extensions in the
/// <c>System.Windows.Markup</c> namespace (runtime model only; the build-time
/// compiler emits these imperatively and does not call <see cref="ProvideValue"/>).
/// </summary>
public abstract class MarkupExtension
{
	/// <summary>
	/// Returns the value that the markup extension provides for the target property.
	/// </summary>
	/// <param name="serviceProvider">
	/// A service provider that can supply context, e.g. <see cref="IProvideValueTarget"/>.
	/// Pass <c>null</c> (or an empty provider) for programmatic use without a XAML context.
	/// </param>
	public abstract object? ProvideValue(IServiceProvider? serviceProvider);
}
