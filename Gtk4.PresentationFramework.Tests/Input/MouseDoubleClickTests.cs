using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>Control.PreviewMouseDoubleClick / MouseDoubleClick raised from ClickCount == 2.</summary>
public class MouseDoubleClickTests : MouseStateTestBase
{
	static readonly Point P = new Point(2, 2);

	[Fact]
	public void DoubleClick_events_are_Direct_owned_by_Control()
	{
		Assert.Equal(RoutingStrategy.Direct, Control.MouseDoubleClickEvent.RoutingStrategy);
		Assert.Equal(RoutingStrategy.Direct, Control.PreviewMouseDoubleClickEvent.RoutingStrategy);
		Assert.Equal(typeof(Control), Control.MouseDoubleClickEvent.OwnerType);
		Assert.Equal(typeof(MouseButtonEventHandler), Control.MouseDoubleClickEvent.HandlerType);
	}

	[Fact]
	public void ClickCount_2_left_raises_Preview_then_MouseDoubleClick()
	{
		var ctrl = new MouseTestControl();
		var log = new List<string>();
		ctrl.PreviewMouseDoubleClick += (s, e) => log.Add("Preview:" + e.ChangedButton);
		ctrl.MouseDoubleClick += (s, e) => log.Add("DoubleClick:" + e.ChangedButton);

		ctrl.RaiseMouseButton(MouseButton.Left, P, 2, pressed: true);

		Assert.Equal(new[] { "Preview:Left", "DoubleClick:Left" }, log);
	}

	[Fact]
	public void ClickCount_1_raises_nothing()
	{
		var ctrl = new MouseTestControl();
		bool fired = false;
		ctrl.PreviewMouseDoubleClick += (s, e) => fired = true;
		ctrl.MouseDoubleClick += (s, e) => fired = true;

		ctrl.RaiseMouseButton(MouseButton.Left, P, 1, pressed: true);

		Assert.False(fired);
	}

	[Fact]
	public void Right_double_click_raises_it_middle_does_not()
	{
		var ctrl = new MouseTestControl();
		var buttons = new List<MouseButton>();
		ctrl.MouseDoubleClick += (s, e) => buttons.Add(e.ChangedButton);

		ctrl.RaiseMouseButton(MouseButton.Right, P, 2, pressed: true);
		ctrl.RaiseMouseButton(MouseButton.Middle, P, 2, pressed: true);

		Assert.Equal(new[] { MouseButton.Right }, buttons);
	}

	[Fact]
	public void Handling_MouseDoubleClick_marks_original_MouseLeftButtonDown_handled()
	{
		var parent = new MouseTestElement("parent");
		var ctrl = new MouseTestControl(parent);
		ctrl.MouseDoubleClick += (s, e) => e.Handled = true;
		bool ctrlLeft = false, parentGeneric = false;
		ctrl.MouseLeftButtonDown += (s, e) => ctrlLeft = true;
		parent.MouseDown += (s, e) => parentGeneric = true;

		ctrl.RaiseMouseButton(MouseButton.Left, P, 2, pressed: true);

		Assert.False(ctrlLeft);
		Assert.False(parentGeneric);
	}

	[Fact]
	public void OnMouseDoubleClick_overrides_are_called()
	{
		var ctrl = new MouseTestControl();
		ctrl.RaiseMouseButton(MouseButton.Left, P, 2, pressed: true);
		Assert.Equal(new[] { "OnPreviewMouseDoubleClick", "OnMouseDoubleClick" }, ctrl.Overrides);
	}

	[Fact]
	public void DoubleClick_args_are_a_new_object_with_same_button_and_click_count()
	{
		var ctrl = new MouseTestControl();
		MouseButtonEventArgs? down = null, dbl = null;
		ctrl.MouseLeftButtonDown += (s, e) => down = e;
		ctrl.MouseDoubleClick += (s, e) => dbl = e;

		ctrl.RaiseMouseButton(MouseButton.Left, P, 2, pressed: true);

		Assert.NotNull(dbl);
		Assert.NotSame(down, dbl);
		Assert.Equal(2, dbl!.ClickCount);
		Assert.Same(Control.MouseDoubleClickEvent, dbl.RoutedEvent);
	}
}
