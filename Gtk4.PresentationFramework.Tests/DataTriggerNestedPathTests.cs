using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// DataTrigger nested binding-path tests: 2-segment activation, leaf/intermediate
/// change, DataContext re-root, and single-segment regression.
/// </summary>
public class DataTriggerNestedPathTests : DataTriggerNestedPathTestBase
{
	[Fact]
	public void Two_segment_path_activates_when_value_matches()
	{
		var vm = new Outer { Child = new Inner { Name = "hit" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm;

		Assert.Equal("hit", el.Tag);
	}

	[Fact]
	public void Two_segment_path_inactive_when_value_does_not_match()
	{
		var vm = new Outer { Child = new Inner { Name = "miss" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm;

		Assert.Null(el.Tag);
	}

	[Fact]
	public void Leaf_change_re_evaluates_trigger()
	{
		var inner = new Inner { Name = "miss" };
		var vm = new Outer { Child = inner };
		var (el, _) = Make2Seg();
		el.DataContext = vm;
		Assert.Null(el.Tag);

		inner.Name = "hit";
		Assert.Equal("hit", el.Tag);
	}

	[Fact]
	public void Leaf_change_deactivates_trigger()
	{
		var inner = new Inner { Name = "hit" };
		var vm = new Outer { Child = inner };
		var (el, _) = Make2Seg();
		el.DataContext = vm;
		Assert.Equal("hit", el.Tag);

		inner.Name = "other";
		Assert.Null(el.Tag);
	}

	[Fact]
	public void Intermediate_reassignment_re_evaluates_trigger()
	{
		var vm = new Outer { Child = new Inner { Name = "miss" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm;
		Assert.Null(el.Tag);

		vm.Child = new Inner { Name = "hit" };
		Assert.Equal("hit", el.Tag);
	}

	[Fact]
	public void Intermediate_reassignment_deactivates_trigger()
	{
		var vm = new Outer { Child = new Inner { Name = "hit" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm;
		Assert.Equal("hit", el.Tag);

		vm.Child = new Inner { Name = "other" };
		Assert.Null(el.Tag);
	}

	[Fact]
	public void DataContext_change_re_roots_and_re_evaluates()
	{
		var vm1 = new Outer { Child = new Inner { Name = "miss" } };
		var vm2 = new Outer { Child = new Inner { Name = "hit" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm1;
		Assert.Null(el.Tag);

		el.DataContext = vm2;
		Assert.Equal("hit", el.Tag);
	}

	[Fact]
	public void DataContext_change_deactivates_when_new_context_does_not_match()
	{
		var vm1 = new Outer { Child = new Inner { Name = "hit" } };
		var vm2 = new Outer { Child = new Inner { Name = "miss" } };
		var (el, _) = Make2Seg();
		el.DataContext = vm1;
		Assert.Equal("hit", el.Tag);

		el.DataContext = vm2;
		Assert.Null(el.Tag);
	}

	[Fact]
	public void Single_segment_path_still_works()
	{
		var vm = new Flat { Active = false };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("Active"), Value = true };
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "yes" });
		style.Triggers.Add(dt);
		el.Style = style;

		vm.Active = true;
		Assert.Equal("yes", el.Tag);
	}
}
