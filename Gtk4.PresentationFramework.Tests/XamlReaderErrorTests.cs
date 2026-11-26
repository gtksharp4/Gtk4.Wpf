using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlReader failure modes.</summary>
public class XamlReaderErrorTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	const string Ns = XamlReaderTests.Ns;

	[Fact]
	public void Unknown_type_throws_with_one_based_position() => Run(() =>
	{
		var ex = Assert.Throws<XamlParseException>(() =>
			XamlReader.Parse("<StackPanel " + Ns + ">\n  <Bogus/></StackPanel>"));
		Assert.Equal(2, ex.LineNumber);
		Assert.Equal(4, ex.LinePosition);
	});

	[Fact]
	public void Unknown_member_and_bad_value_throw_XamlParseException() => Run(() =>
	{
		Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Button " + Ns + " Nope=\"1\"/>"));
		var ex = Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Button " + Ns + " Width=\"abc\"/>"));
		Assert.NotNull(ex.InnerException);
		Assert.Equal(1, ex.LineNumber);
	});

	[Fact]
	public void Event_handlers_and_x_Class_throw() => Run(() =>
	{
		Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Button " + Ns + " Click=\"OnClick\"/>"));
		Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Button " + Ns + " x:Class=\"A.B\"/>"));
	});

	[Fact]
	public void Malformed_xml_missing_resource_and_null_arguments_throw() => Run(() =>
	{
		Assert.Throws<XamlParseException>(() => XamlReader.Parse("<Button"));
		Assert.Throws<XamlParseException>(() =>
			XamlReader.Parse("<Border " + Ns + " Background=\"{StaticResource Missing}\"/>"));
		Assert.Throws<ArgumentNullException>(() => XamlReader.Parse(null!));
		Assert.Throws<ArgumentNullException>(() => XamlReader.Load((Stream)null!));
	});
}
