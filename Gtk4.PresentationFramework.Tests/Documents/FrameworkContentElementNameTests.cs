using System;
using System.Windows;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class FrameworkContentElementNameTests
{
	[Theory]
	[InlineData("1abc")]
	[InlineData("a b")]
	[InlineData("a-b")]
	[InlineData("ʰx")]
	[InlineData("a‿")]
	[InlineData("a‍")]
	public void Invalid_xaml_names_throw_like_wpf(string name)
	{
		var run = new Run();
		Assert.Throws<ArgumentException>(() => run.Name = name);
		Assert.Throws<ArgumentException>(() => run.SetValue(FrameworkContentElement.NameProperty, name));
	}

	[Theory]
	[InlineData("abc")]
	[InlineData("_a1")]
	[InlineData("aʰ")]
	[InlineData("")]
	public void Valid_names_are_accepted(string name)
	{
		var run = new Run { Name = name };
		Assert.Equal(name, run.Name);
	}
}
