using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

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
		var log = new System.Collections.Generic.List<string>();
		var btn = new ProbeButton(log);
		btn.Click += (s, e) => log.Add("handler");
		Click(btn.Widget);
		Assert.Equal(new[] { "OnClick", "handler" }, log);
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
	public void CheckBox_user_toggle_raises_Checked_then_Click_once_and_runs_the_command() => Run(() =>
	{
		var command = new CountingCommand();
		var cb = new CheckBox { Command = command };
		var order = new System.Collections.Generic.List<string>();
		cb.Checked += (s, e) => order.Add("checked");
		cb.Click += (s, e) => order.Add("click");
		((Gtk.CheckButton)cb.Widget).Active = true;
		Assert.Equal(new[] { "checked", "click" }, order);
		Assert.Equal(1, command.Executed);
		Assert.True(cb.IsChecked);
	});

	[Fact]
	public void CheckBox_programmatic_IsChecked_raises_no_Click() => Run(() =>
	{
		var cb = new CheckBox();
		_ = cb.Widget;
		int clicks = 0;
		cb.Click += (s, e) => clicks++;
		cb.IsChecked = true;
		cb.IsChecked = false;
		Assert.Equal(0, clicks);
	});

	[Fact]
	public void ToggleButton_click_raises_Click_once_not_twice() => Run(() =>
	{
		var tb = new ToggleButton();
		int clicks = 0;
		tb.Click += (s, e) => clicks++;
		Click(tb.Widget);
		Assert.Equal(1, clicks);
		Assert.True(tb.IsChecked);
	});

	sealed class CountingCommand : System.Windows.Input.ICommand
	{
		public int Executed;
		public event EventHandler? CanExecuteChanged { add { } remove { } }
		public bool CanExecute(object? parameter) => true;
		public void Execute(object? parameter) => Executed++;
	}

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
		readonly System.Collections.Generic.List<string> _log;

		public ProbeButton(System.Collections.Generic.List<string> log) => _log = log;

		protected override void OnClick()
		{
			_log.Add("OnClick");
			base.OnClick();
		}
	}
}
