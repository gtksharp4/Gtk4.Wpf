using System.Windows.Documents;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for the static command libraries:
/// ApplicationCommands, NavigationCommands, ComponentCommands,
/// MediaCommands, EditingCommands.
/// </summary>
public class CommandLibraryTests
{
	// ── ApplicationCommands ────────────────────────────────────────────────────

	[Fact]
	public void ApplicationCommands_Copy_is_non_null_RoutedUICommand()
	{
		Assert.NotNull(ApplicationCommands.Copy);
		Assert.IsType<RoutedUICommand>(ApplicationCommands.Copy);
	}

	[Fact]
	public void ApplicationCommands_Copy_name_is_Copy()
	{
		Assert.Equal("Copy", ApplicationCommands.Copy.Name);
	}

	[Fact]
	public void ApplicationCommands_Copy_has_CtrlC_gesture()
	{
		var gestures = ApplicationCommands.Copy.InputGestures;
		Assert.NotEmpty(gestures);
		var ctrlC = gestures[0] as KeyGesture;
		Assert.NotNull(ctrlC);
		Assert.Equal(Key.C, ctrlC!.Key);
		Assert.Equal(ModifierKeys.Control, ctrlC.Modifiers);
	}

	[Fact]
	public void ApplicationCommands_Paste_has_CtrlV_gesture()
	{
		var gestures = ApplicationCommands.Paste.InputGestures;
		Assert.NotEmpty(gestures);
		var g = gestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.V, g!.Key);
		Assert.Equal(ModifierKeys.Control, g.Modifiers);
	}

	[Fact]
	public void ApplicationCommands_Undo_has_CtrlZ_gesture()
	{
		var g = ApplicationCommands.Undo.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.Z, g!.Key);
		Assert.Equal(ModifierKeys.Control, g.Modifiers);
	}

	[Fact]
	public void ApplicationCommands_commands_are_distinct_instances()
	{
		Assert.NotSame(ApplicationCommands.Copy, ApplicationCommands.Paste);
		Assert.NotSame(ApplicationCommands.Undo, ApplicationCommands.Redo);
	}

	[Fact]
	public void ApplicationCommands_Find_has_CtrlF_gesture()
	{
		var g = ApplicationCommands.Find.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.F, g!.Key);
		Assert.Equal(ModifierKeys.Control, g.Modifiers);
	}

	// ── NavigationCommands ────────────────────────────────────────────────────

	[Fact]
	public void NavigationCommands_BrowseBack_is_non_null()
	{
		Assert.NotNull(NavigationCommands.BrowseBack);
		Assert.Equal("BrowseBack", NavigationCommands.BrowseBack.Name);
	}

	[Fact]
	public void NavigationCommands_BrowseBack_has_AltLeft_gesture()
	{
		var gestures = NavigationCommands.BrowseBack.InputGestures;
		Assert.NotEmpty(gestures);
		var g = gestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.Left, g!.Key);
		Assert.Equal(ModifierKeys.Alt, g.Modifiers);
	}

	[Fact]
	public void NavigationCommands_Refresh_has_F5_gesture()
	{
		var g = NavigationCommands.Refresh.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.F5, g!.Key);
	}

	[Fact]
	public void NavigationCommands_distinct_from_ApplicationCommands()
	{
		// Two different library's commands are distinct instances.
		Assert.NotSame(ApplicationCommands.Find, NavigationCommands.Search);
	}

	// ── ComponentCommands ─────────────────────────────────────────────────────

	[Fact]
	public void ComponentCommands_MoveLeft_has_Left_gesture()
	{
		var g = ComponentCommands.MoveLeft.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.Left, g!.Key);
		Assert.Equal(ModifierKeys.None, g.Modifiers);
	}

	[Fact]
	public void ComponentCommands_MoveFocusForward_has_Tab_gesture()
	{
		var g = ComponentCommands.MoveFocusForward.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.Tab, g!.Key);
		Assert.Equal(ModifierKeys.None, g.Modifiers);
	}

	[Fact]
	public void ComponentCommands_distinct_from_other_libraries()
	{
		Assert.NotSame(ComponentCommands.MoveLeft, NavigationCommands.BrowseBack);
	}

	// ── MediaCommands ─────────────────────────────────────────────────────────

	[Fact]
	public void MediaCommands_Play_is_non_null()
	{
		Assert.NotNull(MediaCommands.Play);
		Assert.Equal("Play", MediaCommands.Play.Name);
	}

	[Fact]
	public void MediaCommands_Pause_distinct_from_Play()
	{
		Assert.NotSame(MediaCommands.Play, MediaCommands.Pause);
	}

	[Fact]
	public void MediaCommands_distinct_from_ApplicationCommands()
	{
		Assert.NotSame(MediaCommands.Stop, ApplicationCommands.Stop);
	}

	// ── EditingCommands ───────────────────────────────────────────────────────

	[Fact]
	public void EditingCommands_ToggleBold_has_CtrlB_gesture()
	{
		var g = EditingCommands.ToggleBold.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.B, g!.Key);
		Assert.Equal(ModifierKeys.Control, g.Modifiers);
	}

	[Fact]
	public void EditingCommands_AlignLeft_has_CtrlL_gesture()
	{
		var g = EditingCommands.AlignLeft.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.L, g!.Key);
		Assert.Equal(ModifierKeys.Control, g.Modifiers);
	}

	[Fact]
	public void EditingCommands_distinct_from_ApplicationCommands()
	{
		Assert.NotSame(EditingCommands.Delete, ApplicationCommands.Delete);
	}

	[Fact]
	public void EditingCommands_Backspace_has_Back_gesture()
	{
		var g = EditingCommands.Backspace.InputGestures[0] as KeyGesture;
		Assert.NotNull(g);
		Assert.Equal(Key.Back, g!.Key);
	}
}
