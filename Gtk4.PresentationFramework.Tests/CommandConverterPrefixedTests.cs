using System;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>CommandConverter with an IXamlTypeResolver: <c>prefix:Type.Member</c> reads a public static member.</summary>
public class CommandConverterPrefixedTests : WpfGtkTestBase
{
	static Type? Resolve(string name) => name == "a:AnnotationService" ? typeof(AnnotationService) : null;

	[Theory]
	[InlineData("a:AnnotationService.CreateTextStickyNoteCommand")]
	[InlineData(" a:AnnotationService.CreateTextStickyNote ")]
	public void A_prefixed_static_member_is_resolved_with_or_without_the_Command_suffix(string raw) =>
		Assert.Same(AnnotationService.CreateTextStickyNoteCommand, CommandConverters.ParseCommand(raw, Resolve));

	[Theory]
	[InlineData("a:AnnotationService.NoSuchCommand")]
	[InlineData("b:Missing.Open")]
	public void An_unknown_prefixed_member_or_type_is_not_supported(string raw) =>
		Assert.Throws<NotSupportedException>(() => CommandConverters.ParseCommand(raw, Resolve));

	[Fact]
	public void Unprefixed_names_keep_the_library_lookup()
	{
		Assert.Same(ApplicationCommands.Open, CommandConverters.ParseCommand("Open", Resolve));
		Assert.Same(ApplicationCommands.Open, CommandConverters.ParseCommand("ApplicationCommands.Open", Resolve));
		Assert.Null(CommandConverters.ParseCommand("", Resolve));
	}

	[Fact]
	public void XamlReader_resolves_a_prefixed_Command_attribute() => Run(() =>
	{
		var b = (Button)XamlReader.Parse("<Button xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
			+ "xmlns:ann=\"clr-namespace:System.Windows.Annotations\" "
			+ "Command=\"ann:AnnotationService.CreateTextStickyNoteCommand\"/>");

		Assert.Same(AnnotationService.CreateTextStickyNoteCommand, b.Command);
	});
}
