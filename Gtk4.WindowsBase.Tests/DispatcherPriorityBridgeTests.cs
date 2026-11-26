using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Priority-aware BeginInvoke bridge and WPF priority validation.</summary>
public class DispatcherPriorityBridgeTests
{
	[Fact]
	public void BeginInvoke_passes_priority_to_priority_bridge()
	{
		var oldP = Dispatcher.BeginInvokePriorityAction;
		var seen = new List<DispatcherPriority>();
		Dispatcher.BeginInvokePriorityAction = (a, p) => { seen.Add(p); a(); };
		try
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(() => { }, DispatcherPriority.Loaded);
			Dispatcher.CurrentDispatcher.BeginInvoke(() => { });
		}
		finally { Dispatcher.BeginInvokePriorityAction = oldP; }
		Assert.Equal(new[] { DispatcherPriority.Loaded, DispatcherPriority.Normal }, seen);
	}

	[Fact]
	public void BeginInvoke_prefers_priority_bridge_over_legacy_bridge()
	{
		var oldP = Dispatcher.BeginInvokePriorityAction;
		var oldA = Dispatcher.BeginInvokeAction;
		bool legacy = false, prio = false;
		Dispatcher.BeginInvokeAction = a => { legacy = true; a(); };
		Dispatcher.BeginInvokePriorityAction = (a, p) => { prio = true; a(); };
		try { Dispatcher.CurrentDispatcher.BeginInvoke(() => { }); }
		finally { Dispatcher.BeginInvokeAction = oldA; Dispatcher.BeginInvokePriorityAction = oldP; }
		Assert.True(prio);
		Assert.False(legacy);
	}

	[Fact]
	public void BeginInvoke_falls_back_to_legacy_bridge()
	{
		var oldP = Dispatcher.BeginInvokePriorityAction;
		var oldA = Dispatcher.BeginInvokeAction;
		bool legacy = false;
		Dispatcher.BeginInvokePriorityAction = null;
		Dispatcher.BeginInvokeAction = a => { legacy = true; a(); };
		try { Dispatcher.CurrentDispatcher.BeginInvoke(() => { }, DispatcherPriority.Render); }
		finally { Dispatcher.BeginInvokeAction = oldA; Dispatcher.BeginInvokePriorityAction = oldP; }
		Assert.True(legacy);
	}

	[Theory]
	[InlineData(DispatcherPriority.Invalid)]
	[InlineData((DispatcherPriority)11)]
	[InlineData((DispatcherPriority)(-2))]
	public void BeginInvoke_rejects_out_of_range_priority(DispatcherPriority priority)
	{
		Assert.Throws<InvalidEnumArgumentException>(
			() => Dispatcher.CurrentDispatcher.BeginInvoke(() => { }, priority));
	}

	[Fact]
	public void BeginInvoke_Inactive_is_accepted_and_never_runs()
	{
		var oldA = Dispatcher.BeginInvokeAction;
		var oldP = Dispatcher.BeginInvokePriorityAction;
		bool ran = false, legacy = false, prio = false;
		Dispatcher.BeginInvokeAction = a => { legacy = true; a(); };
		Dispatcher.BeginInvokePriorityAction = (a, p) => { prio = true; a(); };
		Exception? ex;
		try
		{
			ex = Record.Exception(
				() => Dispatcher.CurrentDispatcher.BeginInvoke(() => ran = true, DispatcherPriority.Inactive));
		}
		finally { Dispatcher.BeginInvokeAction = oldA; Dispatcher.BeginInvokePriorityAction = oldP; }
		Assert.Null(ex);
		Assert.False(ran);
		Assert.False(legacy);
		Assert.False(prio);
	}

	[Fact]
	public void Setting_BeginInvokeAction_after_priority_bridge_takes_over()
	{
		var oldA = Dispatcher.BeginInvokeAction;
		var oldP = Dispatcher.BeginInvokePriorityAction;
		bool legacy = false, prio = false;
		Dispatcher.BeginInvokePriorityAction = (a, p) => { prio = true; a(); };
		Dispatcher.BeginInvokeAction = a => { legacy = true; a(); };
		Action<Action, DispatcherPriority>? after;
		try
		{
			after = Dispatcher.BeginInvokePriorityAction;
			Dispatcher.CurrentDispatcher.BeginInvoke(() => { }, DispatcherPriority.Normal);
		}
		finally { Dispatcher.BeginInvokeAction = oldA; Dispatcher.BeginInvokePriorityAction = oldP; }
		Assert.True(legacy);
		Assert.False(prio);
		Assert.Null(after);
	}
}
