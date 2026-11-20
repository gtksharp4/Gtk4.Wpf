using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

/// <summary>Routed Click/Checked/Unchecked/Indeterminate through the real GTK signal path.</summary>
public class ButtonBaseEventTests : WpfGtkTestBase
{
	static void Click(Gtk.Widget w) => GLib.Signal.Emit(w, "clicked");

	[Fact]
	public void Hierarchy_matches_WPF()
	{
		Assert.True(typeof(ButtonBase).IsAbstract);
		Assert.Equal(typeof(ContentControl), typeof(ButtonBase).BaseType);
		Assert.Equal(typeof(ButtonBase), typeof(Button).BaseType);
		Assert.Equal(typeof(ButtonBase), typeof(RepeatButton).BaseType);
		Assert.Equal(typeof(ButtonBase), typeof(ToggleButton).BaseType);
		Assert.Equal(typeof(ToggleButton), typeof(CheckBox).BaseType);
		Assert.Equal(typeof(ToggleButton), typeof(RadioButton).BaseType);
	}

	[Fact]
	public void Events_are_routed_bubbling_and_handler_typed()
	{
		Assert.Equal(RoutingStrategy.Bubble, ButtonBase.ClickEvent.RoutingStrategy);
		Assert.Equal(typeof(RoutedEventHandler), ButtonBase.ClickEvent.HandlerType);
		Assert.Equal(RoutingStrategy.Bubble, ToggleButton.CheckedEvent.RoutingStrategy);
		Assert.Equal(typeof(RoutedEventHandler), ToggleButton.UncheckedEvent.HandlerType);
		Assert.Equal(typeof(RoutedEventHandler), ToggleButton.IndeterminateEvent.HandlerType);
		Assert.Equal(typeof(RoutedEventHandler), MenuItem.ClickEvent.HandlerType);
		Assert.Equal(typeof(RoutedEventHandler), MenuItem.CheckedEvent.HandlerType);
		Assert.Equal(typeof(RoutedEventHandler), MenuItem.UncheckedEvent.HandlerType);
	}

	[Fact]
	public void Button_click_bubbles_to_parent_with_original_source() => Run(() =>
	{
		var panel = new StackPanel();
		var btn = new Button();
		panel.Children.Add(btn);
		RoutedEventArgs? seen = null;
		panel.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((s, e) => seen = e));
		int own = 0;
		btn.Click += (s, e) => own++;
		Click(btn.Widget);
		Assert.Equal(1, own);
		Assert.NotNull(seen);
		Assert.Same(btn, seen!.Source);
		Assert.Same(ButtonBase.ClickEvent, seen.RoutedEvent);
	});

	[Fact]
	public void OnClick_virtual_runs_before_handlers() => Run(() =>
	{
		var btn = new ProbeButton();
		Click(btn.Widget);
		Assert.Equal(1, btn.OnClickCalls);
	});

	[Fact]
	public void CheckBox_gtk_toggle_raises_Checked_and_Unchecked_bubbling() => Run(() =>
	{
		var panel = new StackPanel();
		var cb = new CheckBox();
		panel.Children.Add(cb);
		int c = 0, u = 0;
		panel.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler((s, e) => c++));
		panel.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler((s, e) => u++));
		var gtk = (Gtk.CheckButton)cb.Widget;
		gtk.Active = true;
		Assert.Equal(1, c);
		gtk.Active = false;
		Assert.Equal(1, u);
		Assert.Equal(false, cb.IsChecked);
	});

	[Fact]
	public void CheckBox_activate_toggles_once_then_raises_Click_after_Checked() => Run(() =>
	{
		var cb = new CheckBox();
		var order = new System.Collections.Generic.List<string>();
		cb.Checked += (s, e) => order.Add("checked");
		cb.Click += (s, e) => order.Add("click");
		GLib.Signal.Emit(cb.Widget, "activate");
		Assert.Equal(new[] { "checked", "click" }, order);
		Assert.True(cb.IsChecked);
	});

	[Fact]
	public void ThreeState_cycles_and_raises_Indeterminate() => Run(() =>
	{
		var cb = new CheckBox { IsThreeState = true };
		int i = 0;
		cb.Indeterminate += (s, e) => i++;
		var gtk = (Gtk.CheckButton)cb.Widget;
		gtk.Active = true;   // false -> true
		gtk.Active = false;  // true -> null
		Assert.Null(cb.IsChecked);
		Assert.Equal(1, i);
	});

	[Fact]
	public void IsChecked_null_raises_Indeterminate_not_Unchecked()
	{
		var tb = new ToggleButton { IsChecked = true };
		int i = 0, u = 0;
		tb.Indeterminate += (s, e) => i++;
		tb.Unchecked += (s, e) => u++;
		tb.IsChecked = null;
		Assert.Equal(1, i);
		Assert.Equal(0, u);
	}

	[Fact]
	public void MenuItem_Click_and_Checked_are_routed_and_bubble() => Run(() =>
	{
		var panel = new StackPanel();
		var mi = new MenuItem { IsCheckable = true };
		panel.Children.Add(mi);
		int clicks = 0, checks = 0;
		panel.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler((s, e) => clicks++));
		panel.AddHandler(MenuItem.CheckedEvent, new RoutedEventHandler((s, e) => checks++));
		Click(mi.Widget);
		Assert.Equal(1, clicks);
		Assert.Equal(1, checks);
	});

	[Fact]
	public void RadioButton_group_is_exclusive() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44" };
		var b = new RadioButton { GroupName = "g44" };
		var other = new RadioButton { GroupName = "g44other" };
		other.IsChecked = true;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
		Assert.True(other.IsChecked);
	});

	[Fact]
	public void RadioButton_ungrouped_siblings_are_exclusive() => Run(() =>
	{
		var panel = new StackPanel();
		var a = new RadioButton();
		var b = new RadioButton();
		panel.Children.Add(a);
		panel.Children.Add(b);
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
	});

	[Fact]
	public void RadioButton_raises_Checked_and_Unchecked_on_switch() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44ev" };
		var b = new RadioButton { GroupName = "g44ev" };
		int ac = 0, au = 0;
		a.Checked += (s, e) => ac++;
		a.Unchecked += (s, e) => au++;
		a.IsChecked = true;
		b.IsChecked = true;
		Assert.Equal(1, ac);
		Assert.Equal(1, au);
	});

	[Fact]
	public void RadioButton_click_checks_it_and_unchecks_peer() => Run(() =>
	{
		var a = new RadioButton { GroupName = "g44click" };
		var b = new RadioButton { GroupName = "g44click" };
		var ga = (Gtk.CheckButton)a.Widget;
		var gb = (Gtk.CheckButton)b.Widget;
		ga.Active = true;
		gb.Active = true;
		Assert.False(a.IsChecked);
		Assert.True(b.IsChecked);
	});

	[Fact]
	public void WpfStyle_code_behind_handlers_compile()
	{
		var cb = new CheckBox();
		var tb = new ToggleButton();
		var rb = new RadioButton();
		var mi = new MenuItem();
		var btn = new Button();
		var rep = new RepeatButton();
		cb.Checked += OnChecked; cb.Unchecked += OnChecked; cb.Indeterminate += OnChecked;
		tb.Checked += OnChecked; tb.Unchecked += OnChecked;
		rb.Checked += OnChecked; rb.Unchecked += OnChecked;
		mi.Checked += OnChecked; mi.Unchecked += OnChecked; mi.Click += OnChecked;
		btn.Click += OnChecked; rep.Click += OnChecked;
	}

	static void OnChecked(object sender, RoutedEventArgs e) { }

	sealed class ProbeButton : Button
	{
		public int OnClickCalls;
		protected override void OnClick()
		{
			OnClickCalls++;
			base.OnClick();
		}
	}
}
