using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class DispatcherLegacyInvokeTests
{
	[Fact]
	public void Invoke_delegate_runs_at_Send_priority_ahead_of_queued_Normal_work()
	{
		var pump = new DispatcherPump();
		try
		{
			var log = new List<string>();
			bool pendingAfterInvoke = false;
			pump.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
			{
				var queued = pump.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => log.Add("queued")));
				pump.Dispatcher.Invoke(new Action<int>(x => log.Add("inline")), 5);
				pendingAfterInvoke = queued.Status == DispatcherOperationStatus.Pending;
			}));
			Assert.True(pendingAfterInvoke);
			pump.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
			Assert.Equal(new[] { "inline", "queued" }, log);
		}
		finally { pump.Stop(); }
	}
}
