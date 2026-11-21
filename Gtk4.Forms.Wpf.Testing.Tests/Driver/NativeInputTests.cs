using System;
using System.Windows.Automation;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.Wpf.Testing.Tests.Driver;

/// <summary>
/// Real operating-system input through <c>SendInput</c>. Opt-in (<c>XF_NATIVE_INPUT=1</c>, Windows, interactive
/// desktop); not part of the CI gate.
/// </summary>
[Trait("UI", "Native")]
public class NativeInputTests : WpfUiWindowTest<FormWindow>
{
	FormWindow Form => (FormWindow)App.Window;

	[WpfNativeFact]
	public void Native_click_reaches_the_button_through_GTK()
	{
		App.Invoke(() => Form.Login.IsEnabled = true);
		App.WaitForIdle();
		App.Native.Click(App.Find(By.AutomationId("Login")));
		App.WaitUntil(() => Form.Clicks == 1, description: "the native click to raise Button.Click");
		Assert.Equal(1, App.Invoke(() => Form.Clicks));
	}

	[WpfNativeFact]
	public void Native_click_toggles_a_check_box()
	{
		// A wide check button only reacts over its box and label, not the empty space to the right.
		App.Native.Click(App.Find(By.AutomationId("Remember")), at: new System.Windows.Point(30, 8));
		App.WaitUntil(() => Form.Remember.IsChecked == true, description: "the check box to toggle");
	}

	[WpfNativeFact]
	public void Native_typing_enters_text_into_the_focused_text_box()
	{
		App.Native.Click(App.Find(By.AutomationId("User")));
		App.Native.TypeText("ada");
		App.WaitUntil(() => Form.User.Text == "ada", description: "the typed text");
		Assert.True(App.Invoke(() => Form.Login.IsEnabled));
	}

	[WpfUiFact]
	public void Native_input_is_skipped_unless_opted_in() =>
		Assert.Equal(NativeInput.IsSupported && Environment.GetEnvironmentVariable("XF_NATIVE_INPUT") == "1"
			&& WpfUiEnvironment.CanRun, WpfNativeEnvironment.CanRun);
}
