using System;
using System.Windows.Input;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for command plumbing: creates real GTK-backed objects to verify
/// the native widget's sensitivity reflects <see cref="ICommand.CanExecute"/>.
/// </summary>
public class CommandGtkSmokeTests : WpfGtkTestBase
{
	/// <summary>
	/// A Button with Command.CanExecute=false must have an insensitive (disabled) GTK widget.
	/// </summary>
	[Fact]
	public void Button_with_CanExecute_false_is_insensitive() => Run(() =>
	{
		var cmd = new NeverExecuteCommand();
		var btn = new WpfButton { Command = cmd };

		// Materialise the GTK widget.
		var gtkBtn = (Gtk.Button)btn.Widget;

		Assert.False(gtkBtn.Sensitive,
			"GTK button must be insensitive when CanExecute=false.");
	});

	/// <summary>
	/// A Button with Command.CanExecute=true must have a sensitive (enabled) GTK widget.
	/// </summary>
	[Fact]
	public void Button_with_CanExecute_true_is_sensitive() => Run(() =>
	{
		var cmd = new AlwaysExecuteCommand();
		var btn = new WpfButton { Command = cmd };

		var gtkBtn = (Gtk.Button)btn.Widget;

		Assert.True(gtkBtn.Sensitive,
			"GTK button must be sensitive when CanExecute=true.");
	});

	/// <summary>
	/// After InvalidateRequerySuggested the button sensitivity reflects the new CanExecute.
	/// </summary>
	[Fact]
	public void Button_sensitivity_updates_on_InvalidateRequerySuggested() => Run(() =>
	{
		var cmd = new ToggleCommand(initialCanExecute: false);
		var btn = new WpfButton { Command = cmd };
		var gtkBtn = (Gtk.Button)btn.Widget;

		Assert.False(gtkBtn.Sensitive);

		cmd.AllowExecute = true;
		CommandManager.InvalidateRequerySuggested();

		Assert.True(gtkBtn.Sensitive);
	});

	// ── Helper command implementations ────────────────────────────────────────

	sealed class NeverExecuteCommand : ICommand
	{
		public bool CanExecute(object? parameter) => false;
		public void Execute(object? parameter) { }
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
	}

	sealed class AlwaysExecuteCommand : ICommand
	{
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) { }
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
	}

	sealed class ToggleCommand : ICommand
	{
		internal bool AllowExecute { get; set; }

		internal ToggleCommand(bool initialCanExecute)
		{
			AllowExecute = initialCanExecute;
		}

		public bool CanExecute(object? parameter) => AllowExecute;
		public void Execute(object? parameter) { }

		public event EventHandler? CanExecuteChanged
		{
			// Delegate to CommandManager so InvalidateRequerySuggested fires this.
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}
	}
}