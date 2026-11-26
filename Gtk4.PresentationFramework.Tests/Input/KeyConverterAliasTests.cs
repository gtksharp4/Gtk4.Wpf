using System.ComponentModel;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

public class KeyConverterAliasTests
{
	[Theory]
	[InlineData("Ctrl", Key.LeftCtrl)]
	[InlineData("control", Key.LeftCtrl)]
	[InlineData("Esc", Key.Escape)]
	[InlineData("PgUp", Key.PageUp)]
	[InlineData("PgDn", Key.PageDown)]
	[InlineData("F1", Key.F1)]
	[InlineData("Del", Key.Delete)]
	[InlineData("Ins", Key.Insert)]
	[InlineData("BkSp", Key.Back)]
	[InlineData("Plus", Key.OemPlus)]
	[InlineData("NumLock", Key.NumLock)]
	public void Aliases_and_names_convert(string text, Key expected) =>
		Assert.Equal(expected, new KeyConverter().ConvertFromString(text));

	[Fact]
	public void Key_enum_carries_the_KeyConverter() =>
		Assert.IsType<KeyConverter>(TypeDescriptor.GetConverter(typeof(Key)));

	[Fact]
	public void InputEventArgs_is_constructible_and_round_trips()
	{
		var args = new InputEventArgs(Keyboard.PrimaryDevice, 5);
		Assert.Same(Keyboard.PrimaryDevice, args.Device);
		Assert.Equal(5, args.Timestamp);
	}
}
