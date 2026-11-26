using System;
using System.IO;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// V1 (no-lock) and V4 (GetPropertyType) regression tests for
/// <see cref="DpSymbolResolver"/>.
/// </summary>
public class XamlCodeGeneratorResolverDefectTests
{
	static readonly string TestAsmPath =
		typeof(XamlCodeGeneratorSymbolResolverTests).Assembly.Location;

	// ── V4: GetPropertyType for x:Null CLR path ──────────────────────────────

	[Fact]
	public void GetPropertyType_ValueType_ReturnsNonNullType()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		var t = resolver.GetPropertyType(typeof(TestUserControl).FullName!, "MyIntProp");
		Assert.NotNull(t);
		Assert.True(t.IsValueType);
	}

	[Fact]
	public void GetPropertyType_ReferenceType_ReturnsNonNullType()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		var t = resolver.GetPropertyType(typeof(TestUserControl).FullName!, "MyClrProp");
		Assert.NotNull(t);
		Assert.False(t.IsValueType);
	}

	[Fact]
	public void GetPropertyType_NoReferences_ReturnsNull()
	{
		using var resolver = new DpSymbolResolver(null);
		Assert.Null(resolver.GetPropertyType("Any.Type", "AnyProp"));
	}

	// ── V1: no file lock after ReadAllBytes load ──────────────────────────────

	[Fact]
	public void Resolver_DoesNotLockFile_FileCanBeDeletedAfterIsDP()
	{
		// Copy assembly to temp path, resolve against it, assert the copy is deletable.
		string tempPath = Path.Combine(
			Path.GetTempPath(),
			"DpResolverLockTest_" + Guid.NewGuid().ToString("N") + ".dll");
		try
		{
			File.Copy(TestAsmPath, tempPath, overwrite: true);
			using (var resolver = new DpSymbolResolver(new[] { tempPath }))
				_ = resolver.IsDP(typeof(TestUserControl).FullName!, "MyValue");

			// No lock held — delete must succeed.
			File.Delete(tempPath);
			Assert.False(File.Exists(tempPath));
		}
		finally
		{
			if (File.Exists(tempPath))
				try { File.Delete(tempPath); } catch { }
		}
	}

	[Fact]
	public void Resolver_IsDP_StillCorrect_AfterReadAllBytesLoad()
	{
		// Correctness preserved under the no-lock load strategy.
		string tempPath = Path.Combine(
			Path.GetTempPath(),
			"DpResolverCorrectTest_" + Guid.NewGuid().ToString("N") + ".dll");
		File.Copy(TestAsmPath, tempPath, overwrite: true);
		try
		{
			using var resolver = new DpSymbolResolver(new[] { tempPath });
			Assert.True(resolver.IsDP(typeof(TestUserControl).FullName!, "MyValue"));
			Assert.False(resolver.IsDP(typeof(TestUserControl).FullName!, "MyClrProp"));
		}
		finally
		{
			try { File.Delete(tempPath); } catch { }
		}
	}
}