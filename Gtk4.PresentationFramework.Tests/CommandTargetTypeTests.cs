using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>ICommandSource.CommandTarget is an IInputElement, as in WPF, so a sample's own source compiles.</summary>
public class CommandTargetTypeTests
{
	[Fact]
	public void CommandTarget_is_typed_IInputElement_on_the_interface_and_the_sources()
	{
		Assert.Equal(typeof(IInputElement), typeof(ICommandSource).GetProperty("CommandTarget")!.PropertyType);
		Assert.Equal(typeof(IInputElement), typeof(ButtonBase).GetProperty("CommandTarget")!.PropertyType);
		Assert.Equal(typeof(IInputElement), typeof(MenuItem).GetProperty("CommandTarget")!.PropertyType);
		Assert.Equal(typeof(IInputElement), ButtonBase.CommandTargetProperty.PropertyType);
		Assert.Equal(typeof(IInputElement), MenuItem.CommandTargetProperty.PropertyType);
	}
}
