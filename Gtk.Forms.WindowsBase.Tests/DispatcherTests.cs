using System;
using System.Threading;
using System.Windows.Threading;
using Xunit;

namespace Gtk.Forms.WindowsBase.Tests
{
	/// <summary>
	/// Pure tests for <see cref="Dispatcher"/> and <see cref="DispatcherTimer"/>.
	/// No GTK loop required.
	/// </summary>
	public class DispatcherTests
	{
		// ── Dispatcher ───────────────────────────────────────────────────────────────

		[Fact]
		public void CurrentDispatcher_returns_a_Dispatcher()
		{
			var d = Dispatcher.CurrentDispatcher;
			Assert.NotNull(d);
		}

		[Fact]
		public void CurrentDispatcher_stable_on_same_thread()
		{
			var d1 = Dispatcher.CurrentDispatcher;
			var d2 = Dispatcher.CurrentDispatcher;
			Assert.Same(d1, d2);
		}

		[Fact]
		public void Invoke_runs_the_action()
		{
			bool ran = false;
			Dispatcher.CurrentDispatcher.Invoke(() => { ran = true; });
			Assert.True(ran);
		}

		[Fact]
		public void Invoke_T_returns_value()
		{
			var result = Dispatcher.CurrentDispatcher.Invoke(() => 42);
			Assert.Equal(42, result);
		}

		[Fact]
		public void BeginInvoke_runs_the_action()
		{
			bool ran = false;
			Dispatcher.CurrentDispatcher.BeginInvoke(() => { ran = true; });
			Assert.True(ran);
		}

		[Fact]
		public void CheckAccess_true_on_dispatcher_thread()
		{
			// Default IsOnDispatcherThread always returns true.
			Assert.True(Dispatcher.CurrentDispatcher.CheckAccess());
		}

		[Fact]
		public void VerifyAccess_does_not_throw_on_dispatcher_thread()
		{
			var ex = Record.Exception(() => Dispatcher.CurrentDispatcher.VerifyAccess());
			Assert.Null(ex);
		}

		[Fact]
		public void VerifyAccess_throws_when_IsOnDispatcherThread_is_false()
		{
			var saved = Dispatcher.IsOnDispatcherThread;
			try
			{
				Dispatcher.IsOnDispatcherThread = () => false;
				Assert.Throws<InvalidOperationException>(
					() => Dispatcher.CurrentDispatcher.VerifyAccess());
			}
			finally { Dispatcher.IsOnDispatcherThread = saved; }
		}

		[Fact]
		public void Thread_property_is_the_creating_thread()
		{
			Thread? captured = null;
			Thread t = new Thread(() => { captured = Dispatcher.CurrentDispatcher.Thread; });
			t.Start();
			t.Join();
			Assert.Same(t, captured);
		}

		// ── DispatcherPriority ───────────────────────────────────────────────────────

		[Fact]
		public void DispatcherPriority_Normal_has_expected_value()
		{
			Assert.Equal(9, (int)DispatcherPriority.Normal);
		}

		[Fact]
		public void DispatcherPriority_values_are_ordered()
		{
			Assert.True(DispatcherPriority.Background < DispatcherPriority.Normal);
			Assert.True(DispatcherPriority.Normal < DispatcherPriority.Send);
		}

		// ── DispatcherTimer ──────────────────────────────────────────────────────────

		[Fact]
		public void DispatcherTimer_Interval_roundtrips()
		{
			var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
			Assert.Equal(TimeSpan.FromMilliseconds(250), t.Interval);
		}

		[Fact]
		public void DispatcherTimer_IsEnabled_false_by_default()
		{
			var t = new DispatcherTimer();
			Assert.False(t.IsEnabled);
		}

		[Fact]
		public void DispatcherTimer_Tick_fires_via_test_seam()
		{
			int count = 0;
			Action? fireCallback = null;
			var saved = DispatcherTimer.FireActionOverride;
			try
			{
				DispatcherTimer.FireActionOverride = cb => { fireCallback = cb; };
				var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
				timer.Tick += (s, e) => count++;
				timer.Start();

				Assert.NotNull(fireCallback);
				fireCallback!();
				Assert.Equal(1, count);
				fireCallback!();
				Assert.Equal(2, count);
			}
			finally { DispatcherTimer.FireActionOverride = saved; }
		}

		[Fact]
		public void DispatcherTimer_Stop_prevents_further_ticks()
		{
			int count = 0;
			Action? fireCallback = null;
			var saved = DispatcherTimer.FireActionOverride;
			try
			{
				DispatcherTimer.FireActionOverride = cb => { fireCallback = cb; };
				var timer = new DispatcherTimer();
				timer.Tick += (s, e) => count++;
				timer.Start();
				timer.Stop();

				// Fire via the captured callback — should not increment because IsEnabled is false.
				fireCallback?.Invoke();
				Assert.Equal(0, count);
			}
			finally { DispatcherTimer.FireActionOverride = saved; }
		}

		[Fact]
		public void DispatcherTimer_IsEnabled_true_after_Start()
		{
			var saved = DispatcherTimer.FireActionOverride;
			try
			{
				DispatcherTimer.FireActionOverride = _ => { };
				var timer = new DispatcherTimer();
				timer.Start();
				Assert.True(timer.IsEnabled);
				timer.Stop();
			}
			finally { DispatcherTimer.FireActionOverride = saved; }
		}

		[Fact]
		public void DispatcherTimer_IsEnabled_false_after_Stop()
		{
			var saved = DispatcherTimer.FireActionOverride;
			try
			{
				DispatcherTimer.FireActionOverride = _ => { };
				var timer = new DispatcherTimer();
				timer.Start();
				timer.Stop();
				Assert.False(timer.IsEnabled);
			}
			finally { DispatcherTimer.FireActionOverride = saved; }
		}

		[Fact]
		public void DispatcherTimer_FireForTest_does_not_fire_when_stopped()
		{
			int count = 0;
			var timer = new DispatcherTimer();
			timer.Tick += (s, e) => count++;
			// Never started — IsEnabled is false.
			timer.FireForTest();
			Assert.Equal(0, count);
		}

		[Fact]
		public void DispatcherTimer_IsEnabled_setter_starts_and_stops()
		{
			var saved = DispatcherTimer.FireActionOverride;
			try
			{
				DispatcherTimer.FireActionOverride = _ => { };
				var timer = new DispatcherTimer();
				timer.IsEnabled = true;
				Assert.True(timer.IsEnabled);
				timer.IsEnabled = false;
				Assert.False(timer.IsEnabled);
			}
			finally { DispatcherTimer.FireActionOverride = saved; }
		}

		[Fact]
		public void DispatcherTimer_Tag_roundtrips()
		{
			var timer = new DispatcherTimer { Tag = "mytag" };
			Assert.Equal("mytag", timer.Tag);
		}
	}
}
