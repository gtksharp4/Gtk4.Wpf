using System;
using System.Windows.Automation;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Driver;

public class HostTests : WpfUiWindowTest<FormWindow>
{
	FormWindow Form => (FormWindow)App.Window;

	[WpfUiFact]
	public void The_host_runs_the_app_on_the_GTK_thread_and_not_on_the_test_thread()
	{
		Assert.False(GtkThread.IsOnGtkThread);
		Assert.True(App.Invoke(() => GtkThread.IsOnGtkThread));
		Assert.Equal("Form", App.MainWindow.Current.Name);
		Assert.Same(App.Application, App.Invoke(() => System.Windows.Application.Current));
	}

	[WpfUiFact]
	public void Find_locates_by_id_name_control_type_and_class()
	{
		Assert.Equal("Log in", App.Find(By.AutomationId("Login")).Current.Name);
		Assert.Equal("Status", App.Find(By.Name("Signed out")).Current.AutomationId);
		Assert.Equal("Remember", App.Find(By.ControlType(ControlType.CheckBox)).Current.AutomationId);
		Assert.Equal("User", App.Find(By.ClassName("TextBox")).Current.AutomationId);
	}

	[WpfUiFact]
	public void Find_within_an_element_searches_only_below_it()
	{
		var fruit = App.Find(By.AutomationId("Fruit"));
		var veg = App.Find(By.AutomationId("Veg"));
		Assert.Equal(2, App.FindAll(By.Name("Pear")).Length);
		Assert.Single(fruit.FindAll(By.Name("Pear")));
		Assert.Null(fruit.TryFind(By.Name("Leek")));
		Assert.NotNull(veg.TryFind(By.Name("Leek")));
	}

	[WpfUiFact]
	public void A_missing_element_throws_with_the_locator_and_the_tree()
	{
		var error = Assert.Throws<ElementNotFoundException>(() => App.Find(By.AutomationId("Nope")));
		Assert.Contains("AutomationId == \"Nope\"", error.Message);
		Assert.Contains("Window name=\"Form\"", error.Message);
		Assert.Contains("Button id=\"Login\" name=\"Log in\"", error.Message);
		Assert.Contains("[disabled]", error.Message);
	}

	[WpfUiFact]
	public void WaitUntil_pumps_the_loop_so_posted_work_completes()
	{
		bool done = false;
		App.Invoke(() => Dispatcher.CurrentDispatcher.BeginInvoke(() => done = true, DispatcherPriority.Background));
		App.WaitUntil(() => done, TimeSpan.FromSeconds(5), "the posted callback");
		Assert.True(App.Invoke(() => done));
	}

	[WpfUiFact]
	public void WaitUntil_times_out_with_the_description_and_the_tree()
	{
		var error = Assert.Throws<WaitTimeoutException>(
			() => App.WaitUntil(() => false, TimeSpan.FromMilliseconds(100), "the impossible"));
		Assert.Contains("the impossible", error.Message);
		Assert.Contains("Window name=\"Form\"", error.Message);
	}

	[WpfUiFact]
	public void WaitFor_returns_an_element_that_appears_later()
	{
		App.Invoke(() => Dispatcher.CurrentDispatcher.BeginInvoke(
			() => Form.Status.Text = "Ready", DispatcherPriority.Background));
		Assert.Equal("Ready", App.WaitFor(By.Name("Ready")).Current.Name);
	}

	[WpfUiFact]
	public void WaitForIdle_runs_work_queued_at_every_priority()
	{
		int ran = 0;
		App.Invoke(() =>
		{
			foreach (var priority in new[] { DispatcherPriority.Normal, DispatcherPriority.Background,
				DispatcherPriority.ApplicationIdle })
				Dispatcher.CurrentDispatcher.BeginInvoke(() => ran++, priority);
		});
		App.WaitForIdle();
		Assert.Equal(3, App.Invoke(() => ran));
	}

	[WpfUiFact]
	public void Type_enables_the_button_and_Click_invokes_it()
	{
		var login = App.Find(By.AutomationId("Login"));
		Assert.False(login.Current.IsEnabled);
		App.Find(By.AutomationId("User")).Type("ada");
		App.WaitUntil(() => login.Current.IsEnabled);
		login.Click();
		Assert.Equal(1, App.Invoke(() => Form.Clicks));
		Assert.Equal("Welcome, ada", App.Find(By.AutomationId("Status")).GetText());
		Assert.NotNull(App.TryFind(By.Name("Welcome, ada")));
	}

	[WpfUiFact]
	public void Check_and_Uncheck_reach_the_requested_state()
	{
		var box = App.Find(By.AutomationId("Remember"));
		box.Check();
		Assert.True(App.Invoke(() => Form.Remember.IsChecked == true));
		box.Check();
		Assert.True(App.Invoke(() => Form.Remember.IsChecked == true));
		box.Uncheck();
		Assert.True(App.Invoke(() => Form.Remember.IsChecked != true));
	}

	[WpfUiFact]
	public void Select_selects_a_list_item_and_Click_falls_back_to_it()
	{
		var items = App.Find(By.AutomationId("Fruit")).FindAll(By.ControlType(ControlType.ListItem));
		items[0].Select();
		Assert.Equal("Apple", App.Invoke(() => Form.Fruit.SelectedItem));
		items[1].Click();
		Assert.Equal("Pear", App.Invoke(() => Form.Fruit.SelectedItem));
	}

	[WpfUiFact]
	public void An_action_the_element_does_not_support_throws_InvalidOperationException()
	{
		Assert.Throws<InvalidOperationException>(() => App.Find(By.AutomationId("Status")).Check());
		Assert.Throws<InvalidOperationException>(() => App.Find(By.AutomationId("Login")).Expand());
	}

	[WpfUiFact]
	public void Dumping_the_tree_lists_the_desktop_the_window_and_its_children()
	{
		string tree = App.DumpTree();
		Assert.StartsWith("Pane name=\"Desktop\"", tree);
		Assert.Contains("\n  Window name=\"Form\"", tree);
		Assert.Contains("\n    Edit id=\"User\"", tree);
		Assert.Contains("\n      ListItem name=\"Leek\"", tree);
	}

	[WpfUiFact]
	public void WaitForIdle_is_bounded_when_a_zero_interval_timer_is_always_ready()
	{
		var timer = App.Invoke(() =>
		{
			var t = new DispatcherTimer { Interval = TimeSpan.Zero };
			t.Tick += (s, e) => { };
			t.Start();
			return t;
		});
		try
		{
			var clock = System.Diagnostics.Stopwatch.StartNew();
			try { App.WaitForIdle(); }
			catch (WaitTimeoutException error) { Assert.Contains("Window name=\"Form\"", error.Message); }
			Assert.True(clock.Elapsed < TimeSpan.FromSeconds(8), "WaitForIdle outlived its timeout.");
		}
		finally { App.Invoke(timer.Stop); }
	}

	[WpfUiFact]
	public void A_multi_line_name_stays_on_one_line_of_the_dump()
	{
		App.Invoke(() => Form.Status.Text = "first\r\nsecond");
		string tree = App.DumpTree();
		Assert.Contains("name=\"first\\r\\nsecond\"", tree);
		Assert.DoesNotContain("second\"\r", tree);
		Assert.DoesNotContain("\nsecond", tree);
	}
}
