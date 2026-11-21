using System;
using System.Linq;
using System.Windows.Input;
using Xunit;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class SliderClassBindingTests : WpfGtkTestBase
{
	[Fact]
	public void A_new_Slider_has_no_instance_CommandBindings() => Run(() =>
		Assert.Empty(new WpfSlider().CommandBindings));

	[Fact]
	public void Commands_still_work_after_the_instance_bindings_are_cleared() => Run(() =>
	{
		var s = new WpfSlider { Minimum = 0, Maximum = 100, SmallChange = 2, Value = 50 };
		s.CommandBindings.Clear();
		WpfSlider.IncreaseSmall.Execute(null, s);
		Assert.Equal(52, s.Value);
	});

	[Fact]
	public void Default_key_gestures_drive_the_commands() => Run(() =>
	{
		var s = new WpfSlider { Minimum = 0, Maximum = 100, SmallChange = 2, LargeChange = 20, Value = 50 };
		s.RaiseKeyDownForTest(Key.Up);
		Assert.Equal(52, s.Value);
		s.RaiseKeyDownForTest(Key.Left);
		Assert.Equal(50, s.Value);
		s.RaiseKeyDownForTest(Key.PageUp);
		Assert.Equal(70, s.Value);
		s.RaiseKeyDownForTest(Key.End);
		Assert.Equal(100, s.Value);
		s.RaiseKeyDownForTest(Key.Home);
		Assert.Equal(0, s.Value);
	});

	[Fact]
	public void Registration_validates_arguments()
	{
		Assert.Throws<ArgumentNullException>(() => CommandManager.RegisterClassCommandBinding(null!,
			new CommandBinding(ApplicationCommands.Copy)));
		Assert.Throws<ArgumentNullException>(() =>
			CommandManager.RegisterClassInputBinding(typeof(WpfSlider), null!));
	}
}
