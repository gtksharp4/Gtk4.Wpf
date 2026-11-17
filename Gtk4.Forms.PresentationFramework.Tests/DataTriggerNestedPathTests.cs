using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// DataTrigger nested binding-path tests (E5 contract now supported).
/// Covers: 2-segment activation, leaf change, intermediate change,
/// DataContext re-root, single-segment regression, and null-safe traversal.
/// </summary>
public class DataTriggerNestedPathTests
{
	// ── Helpers ──────────────────────────────────────────────────────────────

	sealed class Inner : INotifyPropertyChanged
	{
		string? _name;
		public string? Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class Outer : INotifyPropertyChanged
	{
		Inner? _child;
		public Inner? Child
		{
			get => _child;
			set { _child = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class Flat : INotifyPropertyChanged
	{
		bool _active;
		public bool Active
		{
			get => _active;
			set { _active = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("DtNPTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public string? Tag { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class RecordingAction : TriggerAction
	{
		public int Count { get; private set; }
		internal override void Invoke(FrameworkElement target) => Count++;
	}

	static (TEl el, DataTrigger dt) Make2Seg(string value = "hit")
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var dt = new DataTrigger
		{
			Binding = new Binding("Child.Name"),
			Value = value
		};
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "hit" });
		style.Triggers.Add(dt);
		el.Style = style;
		return (el, dt);
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

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
