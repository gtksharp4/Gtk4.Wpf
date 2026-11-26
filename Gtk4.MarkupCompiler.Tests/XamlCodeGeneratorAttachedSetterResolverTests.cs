using System.Collections.Generic;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Owner with typed attached setters, including a nested and a generic value type.</summary>
class AttachedSetterOwner
{
	public class Nested { }
	public static void SetSize(object target, double value) { }
	public static void SetKind(object target, Nested value) { }
	public static void SetItems(object target, List<int> value) { }
}

public class XamlCodeGeneratorAttachedSetterResolverTests
{
	[Fact]
	public void Resolver_reads_the_setter_value_type()
	{
		using var resolver = new DpSymbolResolver(new[] { XamlCodeGeneratorSymbolResolverTests.TestAsmPath });
		string owner = typeof(AttachedSetterOwner).FullName!;
		Assert.Equal("double", resolver.GetAttachedSetterValueType(owner, "Size"));
		Assert.Equal(typeof(AttachedSetterOwner).FullName + ".Nested", resolver.GetAttachedSetterValueType(owner, "Kind"));
		Assert.Equal("System.Collections.Generic.List<int>", resolver.GetAttachedSetterValueType(owner, "Items"));
		Assert.Null(resolver.GetAttachedSetterValueType(owner, "Missing"));
	}

	[Fact]
	public void Resolver_without_references_returns_null()
	{
		using var resolver = new DpSymbolResolver(null);
		Assert.Null(resolver.GetAttachedSetterValueType("System.Windows.Documents.TextElement", "FontSize"));
	}
}
