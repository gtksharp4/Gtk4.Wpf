using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>
/// WPF-Samples "Input and Commands/HandlingEventOnCommandUsingCode": ApplicationCommands.Open bound in code on the
/// window, run from a button and from Ctrl+R. The sample shows a MessageBox (a dialog); a recording presenter stands
/// in for the GTK dialog.
/// </summary>
public class CommandSampleTests : WpfUiWindowTest<HandlingEventOnCommandUsingCode.MainWindow>
{
	sealed class RecordingPresenter : IMessageBoxPresenter
	{
		public string Text;
		public int Shown;

		public MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
		{
			Text = messageText;
			Shown++;
			return MessageBoxResult.OK;
		}
	}

	const string Expected = "The command has been invoked.";

	readonly IMessageBoxPresenter _previous = MessageBox.Presenter;
	readonly RecordingPresenter _presenter = new RecordingPresenter();

	public CommandSampleTests() => App.Invoke(() => MessageBox.Presenter = _presenter);

	public override void Dispose()
	{
		App.Invoke(() => MessageBox.Presenter = _previous);
		base.Dispose();
	}

	AutomationElement Button => App.WaitFor(By.ControlType(ControlType.Button));

	// The sample adds its button and bindings in Loaded. WPF re-queries commands after such changes; the layer
	// does not do it on its own yet, so the test asks for it, as input or focus changes would.
	void Requery() => App.Invoke(CommandManager.InvalidateRequerySuggested);

	[WpfUiFact]
	public void The_Loaded_handler_adds_the_command_button()
	{
		Assert.Equal("Open (KeyBindings: Ctrl-R, Ctrl-0)", Button.Current.Name);
	}

	[WpfUiFact]
	public void The_button_is_enabled_once_commands_are_requeried_and_Invoke_runs_the_command()
	{
		var button = Button;
		Requery();
		App.WaitUntil(() => button.Current.IsEnabled, description: "the CanExecute handler to enable the button");
		button.Click();
		Assert.Equal(Expected, _presenter.Text);
		Assert.Equal(1, _presenter.Shown);
	}

	/// <summary>WPF re-queries commands after the sample's Loaded handler adds the binding, so the button enables on its own.</summary>
	[WpfUiFact]
	public void The_button_is_enabled_without_a_manual_requery()
	{
		var button = Button;
		App.WaitUntil(() => button.Current.IsEnabled, description: "the CanExecute handler to enable the button");
	}

	[WpfUiFact]
	public void Ctrl_R_runs_the_command_through_the_KeyBinding()
	{
		_ = Button;
		App.Input.Keyboard.Press(Key.R, ModifierKeys.Control, App.MainWindow);
		Assert.Equal(Expected, _presenter.Text);
	}

	[WpfUiFact]
	public void R_alone_does_not_run_it()
	{
		_ = Button;
		App.Input.Keyboard.Press(Key.R, ModifierKeys.None, App.MainWindow);
		Assert.Null(_presenter.Text);
	}
}
