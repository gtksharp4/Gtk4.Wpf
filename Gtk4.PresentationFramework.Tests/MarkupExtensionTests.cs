using System;
using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Tests for the runtime <see cref="MarkupExtension"/> model.</summary>
public class MarkupExtensionTests
{
	sealed class MEl : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── StaticResourceExtension ──────────────────────────────────────────────

	[Fact]
	public void StaticResource_resolves_from_element_resources()
	{
		var el = new MEl();
		el.Resources["myKey"] = "hello";

		var ext = new StaticResourceExtension("myKey");
		var svc = new FakeServiceProvider(el);

		var result = ext.ProvideValue(svc);

		Assert.Equal("hello", result);
	}

	[Fact]
	public void StaticResource_resolves_from_application_resources_when_no_element()
	{
		// Use the same MockApplication pattern as ResourceStyleTests.
		var app = new TestApplication();
		app.Resources["appKey"] = 42;
		try
		{
			var ext = new StaticResourceExtension("appKey");
			var result = ext.ProvideValue(null);
			Assert.Equal(42, result);
		}
		finally
		{
			app.Resources.Clear();
		}
	}

	[Fact]
	public void StaticResource_throws_when_key_not_found()
	{
		var el = new MEl();
		var ext = new StaticResourceExtension("missing");
		var svc = new FakeServiceProvider(el);
		Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(svc));
	}

	// ── TypeExtension ────────────────────────────────────────────────────────

	[Fact]
	public void TypeExtension_returns_type_from_Type_property()
	{
		var ext = new TypeExtension(typeof(string));
		var result = ext.ProvideValue(null);
		Assert.Equal(typeof(string), result);
	}

	[Fact]
	public void TypeExtension_returns_type_from_assembly_qualified_name()
	{
		var ext = new TypeExtension(typeof(int).AssemblyQualifiedName!);
		var result = ext.ProvideValue(null);
		Assert.Equal(typeof(int), result);
	}

	[Fact]
	public void TypeExtension_throws_when_name_cannot_be_resolved()
	{
		var ext = new TypeExtension("NoSuchType.DoesNotExist");
		Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(null));
	}

	// ── NullExtension ────────────────────────────────────────────────────────

	[Fact]
	public void NullExtension_returns_null()
	{
		var ext = new NullExtension();
		Assert.Null(ext.ProvideValue(null));
	}

	// ── StaticExtension ──────────────────────────────────────────────────────

	[Fact]
	public void StaticExtension_returns_public_static_field_value()
	{
		// Use Int32.MaxValue as a well-known static field.
		var ext = new StaticExtension(typeof(int).FullName + ".MaxValue");
		var result = ext.ProvideValue(null);
		Assert.Equal(int.MaxValue, result);
	}

	[Fact]
	public void StaticExtension_returns_public_static_property_value()
	{
		// Environment.NewLine is a static property.
		var ext = new StaticExtension(typeof(Environment).FullName + ".NewLine");
		var result = ext.ProvideValue(null);
		Assert.Equal(Environment.NewLine, result);
	}

	[Fact]
	public void StaticExtension_throws_when_member_not_found()
	{
		var ext = new StaticExtension(typeof(string).FullName + ".NoSuchMember");
		Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(null));
	}

	// ── ArrayExtension ───────────────────────────────────────────────────────

	[Fact]
	public void ArrayExtension_builds_typed_array()
	{
		var ext = new ArrayExtension(typeof(int));
		ext.Items.Add(1);
		ext.Items.Add(2);
		ext.Items.Add(3);

		var result = ext.ProvideValue(null) as int[];

		Assert.NotNull(result);
		Assert.Equal(new[] { 1, 2, 3 }, result);
	}

	[Fact]
	public void ArrayExtension_builds_empty_array_when_no_items()
	{
		var ext = new ArrayExtension(typeof(string));
		var result = ext.ProvideValue(null) as string[];
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	[Fact]
	public void ArrayExtension_throws_when_Type_not_set()
	{
		var ext = new ArrayExtension();
		Assert.Throws<InvalidOperationException>(() => ext.ProvideValue(null));
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	/// <summary>Stub Application (skips GTK init) — same pattern as ResourceStyleTests.</summary>
	sealed class TestApplication : System.Windows.Application { }

	sealed class FakeServiceProvider : IServiceProvider
	{
		readonly FrameworkElement _target;
		internal FakeServiceProvider(FrameworkElement target) => _target = target;

		public object? GetService(Type serviceType)
		{
			if (serviceType == typeof(IProvideValueTarget))
				return new PvtImpl(_target);
			return null;
		}

		sealed class PvtImpl : IProvideValueTarget
		{
			internal PvtImpl(FrameworkElement t) { TargetObject = t; }
			public object? TargetObject { get; }
			public object? TargetProperty => null;
		}
	}
}
