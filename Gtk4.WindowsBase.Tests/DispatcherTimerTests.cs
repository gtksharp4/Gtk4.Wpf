using System;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>
/// Pure tests for <see cref="DispatcherTimer"/>.
/// No GTK loop required.
/// </summary>
public class DispatcherTimerTests
{
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