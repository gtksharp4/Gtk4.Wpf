using System;
using Xunit;

using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public class NullExtensionTests : BaseTestFixture
{
	[Fact]
	public void TestxNull()
	{
		var markupString = "{x:Null}";
		var serviceProvider = new Internals.XamlServiceProvider(null, null);
		var result = (new MarkupExtensionParser()).ParseExpression(ref markupString, serviceProvider);

		Assert.Null(result);
	}
}