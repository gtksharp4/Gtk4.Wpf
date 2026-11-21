using System.Windows.Automation;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.Wpf.Testing.Tests.Driver;

public class InputTests : WpfUiWindowTest<FormWindow>
{
	FormWindow Form => (FormWindow)App.Window;

	[WpfUiFact]
	public void Keyboard_Press_raises_KeyDown_on_the_target_element()
	{
		Key seen = Key.None;
		ModifierKeys mods = ModifierKeys.None;
		App.Invoke(() => Form.User.KeyDown += (s, e) =>
		{
			seen = e.Key;
			mods = e.Modifiers;
		});
		App.Input.Keyboard.Press(Key.R, ModifierKeys.Control, App.Find(By.AutomationId("User")));
		Assert.Equal(Key.R, seen);
		Assert.Equal(ModifierKeys.Control, mods);
		Assert.Equal(ModifierKeys.None, App.Invoke(() => Keyboard.Modifiers));
		Assert.Equal(1, App.Invoke(() => Form.KeyDowns));
	}

	[WpfUiFact]
	public void Keyboard_without_a_target_goes_to_the_focused_element_and_bubbles_to_the_window()
	{
		int windowKeys = 0;
		int checkKeys = 0;
		App.Invoke(() =>
		{
			Form.KeyDown += (s, e) => windowKeys++;
			Form.Remember.KeyDown += (s, e) => checkKeys++;
			Form.Remember.Focus();
		});
		App.Input.Keyboard.Press(Key.B);
		Assert.Equal(1, checkKeys);
		Assert.Equal(1, windowKeys);
		Assert.Equal(0, App.Invoke(() => Form.KeyDowns));
	}

	[WpfUiFact]
	public void Keyboard_Down_and_Up_are_separate_events()
	{
		int up = 0;
		App.Invoke(() => Form.User.KeyUp += (s, e) => up++);
		var user = App.Find(By.AutomationId("User"));
		App.Input.Keyboard.Down(Key.Z, target: user);
		Assert.Equal(0, up);
		App.Input.Keyboard.Up(Key.Z, target: user);
		Assert.Equal(1, up);
	}

	[WpfUiFact]
	public void Mouse_Click_raises_MouseDown_and_MouseUp()
	{
		int ups = 0;
		App.Invoke(() => Form.Login.MouseUp += (s, e) => ups++);
		App.Input.Mouse.Click(App.Find(By.AutomationId("Login")));
		Assert.Equal(1, App.Invoke(() => Form.MouseDowns));
		Assert.Equal(1, ups);
	}

	[WpfUiFact]
	public void Mouse_MoveTo_and_Leave_toggle_IsMouseOver()
	{
		var user = App.Find(By.AutomationId("User"));
		App.Input.Mouse.MoveTo(user);
		Assert.True(App.Invoke(() => Form.User.IsMouseOver));
		App.Input.Mouse.Leave(user);
		Assert.False(App.Invoke(() => Form.User.IsMouseOver));
	}

	[WpfUiFact]
	public void Mouse_accepts_an_element_with_no_peer()
	{
		int enters = 0;
		App.Invoke(() => Form.Status.MouseEnter += (s, e) => enters++);
		App.Input.Mouse.MoveTo(Form.Status);
		Assert.Equal(1, enters);
	}
}
