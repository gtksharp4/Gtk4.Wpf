using System;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class XamlParseExceptionTests
{
	[Fact]
	public void Derives_from_SystemException() =>
		Assert.IsAssignableFrom<SystemException>(new XamlParseException());

	[Fact]
	public void Position_constructors_keep_line_position_and_inner_exception()
	{
		var inner = new InvalidOperationException("inner");
		var ex = new XamlParseException("boom", 3, 9, inner);
		Assert.Equal("boom", ex.Message);
		Assert.Equal(3, ex.LineNumber);
		Assert.Equal(9, ex.LinePosition);
		Assert.Same(inner, ex.InnerException);
		var plain = new XamlParseException("m", 1, 2);
		Assert.Equal((1, 2), (plain.LineNumber, plain.LinePosition));
		Assert.Same(inner, new XamlParseException("m", inner).InnerException);
		Assert.Equal("m", new XamlParseException("m").Message);
	}

	[Fact]
	public void Context_properties_default_to_null_and_BaseUri_has_no_public_setter()
	{
		var ex = new XamlParseException();
		Assert.Null(ex.BaseUri);
		Assert.Null(ex.KeyContext);
		Assert.Null(ex.NameContext);
		Assert.Null(ex.UidContext);
		Assert.False(typeof(XamlParseException).GetProperty("BaseUri")!.SetMethod!.IsPublic);
	}
}
