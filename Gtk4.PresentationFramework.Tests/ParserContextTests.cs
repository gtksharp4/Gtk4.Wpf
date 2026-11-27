using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlReader.Load(Stream, ParserContext): the context's BaseUri reaches IUriContext objects.</summary>
public class ParserContextTests : WpfGtkTestBase
{
	const string Ns = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";

	static MemoryStream Xaml(string text) => new MemoryStream(Encoding.UTF8.GetBytes(text));

	[Fact]
	public void BaseUri_is_given_to_IUriContext_objects() => Run(() =>
	{
		var context = new ParserContext { BaseUri = new Uri("file:///samples/dir/") };

		var timeline = (MediaTimeline)XamlReader.Load(Xaml("<MediaTimeline " + Ns + " Source=\"a.wmv\"/>"), context);

		Assert.Equal(context.BaseUri, ((IUriContext)timeline).BaseUri);
	});

	[Fact]
	public void Without_a_context_BaseUri_stays_unset() => Run(() =>
	{
		var timeline = (MediaTimeline)XamlReader.Load(Xaml("<MediaTimeline " + Ns + " Source=\"a.wmv\"/>"));

		Assert.Null(((IUriContext)timeline).BaseUri);
	});

	[Fact]
	public void A_null_context_is_allowed_but_a_null_stream_is_rejected() => Run(() =>
	{
		Assert.NotNull(XamlReader.Load(Xaml("<Button " + Ns + "/>"), null!));
		Assert.Throws<ArgumentNullException>(() => XamlReader.Load((Stream)null!, new ParserContext()));
	});

	[Fact]
	public void The_XmlParserContext_constructor_carries_BaseURI_and_converts_back()
	{
		var xml = new System.Xml.XmlParserContext(null, null, "fr-FR", System.Xml.XmlSpace.Preserve)
		{
			BaseURI = "file:///x/y/"
		};

		var context = new ParserContext(xml);

		Assert.Equal(new Uri("file:///x/y/"), context.BaseUri);
		Assert.Equal("fr-FR", context.XmlLang);
		Assert.Equal("preserve", context.XmlSpace);
		Assert.IsAssignableFrom<IUriContext>(context);
		context.BaseUri = new Uri("file:///z/");
		var back = ParserContext.ToXmlParserContext(context);
		Assert.Equal("file:///z/", back.BaseURI);
		Assert.Equal(System.Xml.XmlSpace.Preserve, back.XmlSpace);
	}
}
