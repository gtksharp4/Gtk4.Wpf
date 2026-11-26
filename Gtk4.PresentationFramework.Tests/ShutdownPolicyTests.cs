using System;
using System.Collections;
using System.Linq;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Pure tests for <see cref="ShutdownMode"/> policy and the small lifecycle argument types.</summary>
public class ShutdownPolicyTests
{
	[Theory]
	[InlineData(ShutdownMode.OnLastWindowClose, true, 0, true)]
	[InlineData(ShutdownMode.OnLastWindowClose, true, 1, false)]
	[InlineData(ShutdownMode.OnLastWindowClose, true, 3, false)]
	[InlineData(ShutdownMode.OnLastWindowClose, false, 0, true)]
	[InlineData(ShutdownMode.OnLastWindowClose, false, 1, false)]
	[InlineData(ShutdownMode.OnLastWindowClose, false, 3, false)]
	[InlineData(ShutdownMode.OnMainWindowClose, true, 0, true)]
	[InlineData(ShutdownMode.OnMainWindowClose, true, 1, true)]
	[InlineData(ShutdownMode.OnMainWindowClose, true, 3, true)]
	[InlineData(ShutdownMode.OnMainWindowClose, false, 0, false)]
	[InlineData(ShutdownMode.OnMainWindowClose, false, 1, false)]
	[InlineData(ShutdownMode.OnMainWindowClose, false, 3, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, true, 0, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, true, 1, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, true, 3, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, false, 0, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, false, 1, false)]
	[InlineData(ShutdownMode.OnExplicitShutdown, false, 3, false)]
	public void ShouldShutdown_matrix(ShutdownMode mode, bool closingIsMain, int remaining, bool expected) =>
		Assert.Equal(expected, ShutdownPolicy.ShouldShutdown(mode, closingIsMain, remaining));

	[Fact]
	public void ShutdownMode_values_match_wpf()
	{
		Assert.Equal(0, (int)ShutdownMode.OnLastWindowClose);
		Assert.Equal(1, (int)ShutdownMode.OnMainWindowClose);
		Assert.Equal(2, (int)ShutdownMode.OnExplicitShutdown);
	}

	[Fact]
	public void StartupEventArgs_Args_is_never_null() =>
		Assert.Empty(new StartupEventArgs(null).Args);

	[Fact]
	public void ProcessArgs_excludes_the_executable()
	{
		var expected = Environment.GetCommandLineArgs().Skip(1).ToArray();
		Assert.Equal(expected, StartupEventArgs.ProcessArgs());
	}

	[Fact]
	public void ExitEventArgs_code_round_trips()
	{
		var e = new ExitEventArgs(3);
		Assert.Equal(3, e.ApplicationExitCode);
		e.ApplicationExitCode = 5;
		Assert.Equal(5, e.ApplicationExitCode);
	}

	[Theory]
	[InlineData(ReasonSessionEnding.Logoff)]
	[InlineData(ReasonSessionEnding.Shutdown)]
	public void SessionEnding_args_carry_reason_and_cancel(ReasonSessionEnding reason)
	{
		var e = new SessionEndingCancelEventArgs(reason);
		Assert.Equal(reason, e.ReasonSessionEnding);
		Assert.False(e.Cancel);
		e.Cancel = true;
		Assert.True(e.Cancel);
	}
}

/// <summary>Contract tests for <see cref="WindowCollection"/>.</summary>
public class WindowCollectionTests : WpfGtkTestBase
{
	static Window[] Make(int n) => Enumerable.Range(0, n).Select(_ => new Window()).ToArray();

	[Fact]
	public void Count_indexer_and_enumerator_reflect_the_snapshot() => Run(() =>
	{
		var ws = Make(3);
		var c = new WindowCollection(ws);

		Assert.Equal(3, c.Count);
		Assert.Same(ws[1], c[1]);
		Assert.Equal(ws, c.Cast<Window>().ToArray());
		Assert.False(c.IsSynchronized);
		Assert.NotNull(c.SyncRoot);
	});

	[Fact]
	public void CopyTo_copies_at_offset() => Run(() =>
	{
		var ws = Make(2);
		var dest = new Window[4];
		new WindowCollection(ws).CopyTo(dest, 1);

		Assert.Null(dest[0]);
		Assert.Same(ws[0], dest[1]);
		Assert.Same(ws[1], dest[2]);
	});

	[Fact]
	public void CopyTo_validates_bounds() => Run(() =>
	{
		var c = new WindowCollection(Make(2));

		Assert.Throws<ArgumentException>(() => c.CopyTo(new Window[1], 0));
		Assert.Throws<ArgumentException>(() => c.CopyTo(new Window[3], 2));
		Assert.Throws<ArgumentOutOfRangeException>(() => c.CopyTo(new Window[3], -1));
	});

	[Fact]
	public void ICollection_CopyTo_validates_bounds() => Run(() =>
	{
		ICollection c = new WindowCollection(Make(2));

		var dest = new Window[2];
		c.CopyTo(dest, 0);
		Assert.NotNull(dest[1]);
		Assert.Throws<ArgumentException>(() => c.CopyTo(new Window[1], 0));
	});

	[Fact]
	public void Indexer_out_of_range_throws() => Run(() =>
		Assert.Throws<ArgumentOutOfRangeException>(() => new WindowCollection(Make(1))[1]));
}
