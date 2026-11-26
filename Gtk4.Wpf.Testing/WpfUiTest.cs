using System;
using System.Windows;
using Xunit;

namespace Gtk4.Wpf.Testing;

/// <summary>
/// Serialises every UI test class: they share one GTK thread and process-wide WPF state. Add
/// <c>[assembly: CollectionBehavior(DisableTestParallelization = true)]</c> too when other tests run alongside.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class WpfUiCollection
{
	public const string Name = "Gtk4.Wpf.Testing";
}

/// <summary>
/// Base class of a UI test class: starts the app before each test and, when the test fails, saves the
/// automation tree and a PNG of the window to <see cref="WpfAppHost.EvidenceDirectory"/>.
/// </summary>
[Collection(WpfUiCollection.Name)]
public abstract class WpfUiTest : IDisposable
{
	protected WpfUiTest(WpfAppHost app) => App = app ?? throw new ArgumentNullException(nameof(app));

	/// <summary>The app under test.</summary>
	protected WpfAppHost App { get; }

	public virtual void Dispose()
	{
		try
		{
			if (TestContext.Current.TestState?.Result == TestResult.Failed)
				foreach (string file in App.SaveEvidence(TestContext.Current.Test?.TestDisplayName ?? "failed-test"))
					TestContext.Current.SendDiagnosticMessage("UI evidence: " + file);
		}
		finally { App.Dispose(); }
	}
}

/// <summary>Starts <typeparamref name="TApp"/> in process for each test.</summary>
public abstract class WpfUiTest<TApp> : WpfUiTest where TApp : Application
{
	protected WpfUiTest() : base(WpfAppHost.Start<TApp>())
	{
	}
}

/// <summary>Shows a new <typeparamref name="TWindow"/> in a fresh <see cref="Application"/> for each test.</summary>
public abstract class WpfUiWindowTest<TWindow> : WpfUiTest where TWindow : Window, new()
{
	protected WpfUiWindowTest() : base(WpfAppHost.Start(() => new TWindow()))
	{
	}
}
