using System;
using System.Linq;
using System.Windows.Input;
using Xunit;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.PresentationFramework.Tests.Controls;

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
	public void A_gesture_the_class_handles_is_reported_handled_to_GTK() => Run(() =>
	{
		var s = new WpfSlider { Minimum = 0, Maximum = 100, SmallChange = 2, LargeChange = 20, Value = 50 };
		_ = s.Widget;
		var controller = s._keyController!;
		// GTK's own Range bindings run when key-pressed is not handled; a handled key must stop them.
		Assert.True(EmitKeyPressed(controller, 0xFF52)); // Up
		Assert.Equal(52, s.Value);
		Assert.True(EmitKeyPressed(controller, 0xFF55)); // Page_Up
		Assert.Equal(72, s.Value);
		Assert.False(EmitKeyPressed(controller, 0x61)); // 'a': no gesture
	});

	static bool EmitKeyPressed(Gtk.EventControllerKey controller, uint keyval) =>
		(bool)GLib.Signal.Emit(controller, "key-pressed", keyval, 0u, Gdk.ModifierType.None)!;

	[Fact]
	public void Registration_validates_arguments()
	{
		Assert.Throws<ArgumentNullException>(() => CommandManager.RegisterClassCommandBinding(null!,
			new CommandBinding(ApplicationCommands.Copy)));
		Assert.Throws<ArgumentNullException>(() =>
			CommandManager.RegisterClassInputBinding(typeof(WpfSlider), null!));
	}
}
