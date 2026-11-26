using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// DataTrigger nested-path null-traversal and EnterActions/ExitActions behaviour.
/// </summary>
public class DataTriggerNestedPathActionsTests : DataTriggerNestedPathTestBase
{
	[Fact]
	public void Null_intermediate_does_not_throw_and_trigger_is_inactive()
	{
		var vm = new Outer { Child = null }; // Child is null — no throw expected
		var (el, _) = Make2Seg();
		el.DataContext = vm;

		Assert.Null(el.Tag); // inactive, not an exception
	}

	[Fact]
	public void Null_intermediate_becoming_non_null_activates_trigger()
	{
		var vm = new Outer { Child = null };
		var (el, _) = Make2Seg();
		el.DataContext = vm;
		Assert.Null(el.Tag);

		vm.Child = new Inner { Name = "hit" };
		Assert.Equal("hit", el.Tag);
	}

	[Fact]
	public void EnterActions_fire_on_nested_path_activation()
	{
		var enter = new RecordingAction();
		var inner = new Inner { Name = "miss" };
		var vm = new Outer { Child = inner };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("Child.Name"), Value = "hit" };
		dt.EnterActions.Add(enter);
		style.Triggers.Add(dt);
		el.Style = style;

		Assert.Equal(0, enter.Count);
		inner.Name = "hit";
		Assert.Equal(1, enter.Count);
	}

	[Fact]
	public void ExitActions_fire_on_nested_path_deactivation()
	{
		var exit = new RecordingAction();
		var inner = new Inner { Name = "hit" };
		var vm = new Outer { Child = inner };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("Child.Name"), Value = "hit" };
		dt.ExitActions.Add(exit);
		style.Triggers.Add(dt);
		el.Style = style;

		Assert.Equal(0, exit.Count);
		inner.Name = "other";
		Assert.Equal(1, exit.Count);
	}
}
