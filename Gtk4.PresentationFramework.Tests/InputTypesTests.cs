using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class InputTypesTests : WpfGtkTestBase
{
	[Fact]
	public void RoutedCommand_parameterless_constructor_gives_an_unnamed_command() => Run(() =>
	{
		var c = new RoutedCommand();
		Assert.Equal(string.Empty, c.Name);
		Assert.Null(c.OwnerType);
		Assert.Empty(c.InputGestures);
		var ui = new RoutedUICommand();
		Assert.Equal(string.Empty, ui.Text);
		Assert.Equal(string.Empty, ui.Name);
	});

	[Fact]
	public void RoutedCommand_named_constructor_validates_its_arguments() => Run(() =>
	{
		Assert.Throws<ArgumentNullException>(() => new RoutedCommand(null!, typeof(string)));
		Assert.Throws<ArgumentException>(() => new RoutedCommand("", typeof(string)));
		Assert.Throws<ArgumentNullException>(() => new RoutedCommand("x", null!));
		var c = new RoutedCommand("Go", typeof(string));
		Assert.Equal("Go", c.Name);
		Assert.Equal(typeof(string), c.OwnerType);
	});

	[Fact]
	public void A_parameterless_RoutedCommand_executes_through_a_CommandBinding() => Run(() =>
	{
		var cmd = new RoutedCommand();
		var button = new Button();
		int ran = 0;
		button.CommandBindings.Add(new CommandBinding(cmd, (_, _) => ran++, (_, e) => e.CanExecute = true));
		Assert.True(cmd.CanExecute(null, button));
		cmd.Execute(null, button);
		Assert.Equal(1, ran);
	});

	[Fact]
	public void EditingCommands_live_in_System_Windows_Documents_with_the_full_command_set() => Run(() =>
	{
		Assert.Equal("System.Windows.Documents", typeof(EditingCommands).Namespace);
		Assert.Equal("MoveUpByLine", EditingCommands.MoveUpByLine.Name);
		Assert.Equal(typeof(EditingCommands), EditingCommands.MoveUpByLine.OwnerType);
		var all = typeof(EditingCommands).GetProperties().Where(p => p.PropertyType == typeof(RoutedUICommand))
			.Select(p => (RoutedUICommand)p.GetValue(null)!).ToList();
		Assert.True(all.Count >= 55, $"only {all.Count} commands");
		Assert.All(all, c => Assert.NotNull(c.OwnerType));
		Assert.Equal(all.Count, all.Select(c => c.Name).Distinct().Count());
		Assert.Contains(Key.Home, new[] { ((KeyGesture)EditingCommands.MoveToLineStart.InputGestures[0]).Key });
		Assert.NotNull(EditingCommands.ToggleInsert);
	});

	[Fact]
	public void Editing_commands_run_on_a_target_with_a_binding() => Run(() =>
	{
		var box = new TextBox();
		int ran = 0;
		box.CommandBindings.Add(new CommandBinding(EditingCommands.AlignCenter, (_, _) => ran++,
			(_, e) => e.CanExecute = true));
		EditingCommands.AlignCenter.Execute(null, box);
		Assert.Equal(1, ran);
	});

	[Fact]
	public void Cursors_has_the_WPF_set_including_ScrollWE() => Run(() =>
	{
		string[] names =
		{
			"AppStarting", "Arrow", "ArrowCD", "Cross", "Hand", "Help", "IBeam", "No", "None", "Pen", "ScrollAll",
			"ScrollE", "ScrollN", "ScrollNE", "ScrollNS", "ScrollNW", "ScrollS", "ScrollSE", "ScrollSW", "ScrollW",
			"ScrollWE", "SizeAll", "SizeNESW", "SizeNS", "SizeNWSE", "SizeWE", "UpArrow", "Wait",
		};
		foreach (var n in names)
		{
			var p = typeof(Cursors).GetProperty(n);
			Assert.NotNull(p);
			Assert.NotNull(p!.GetValue(null));
		}
		Assert.Equal("ew-resize", Cursors.ScrollWE.ToString());
	});

	[Fact]
	public void Cursor_is_disposable_and_file_cursors_fall_back() => Run(() =>
	{
		var c = new Cursor("missing.cur");
		Assert.IsAssignableFrom<IDisposable>(c);
		c.Dispose();
		c.Dispose();
		Assert.Equal("default", new Cursor("missing.cur", true).ToString());
		Cursors.Hand.Dispose();
		Assert.Equal("pointer", Cursors.Hand.ToString());
	});
}
