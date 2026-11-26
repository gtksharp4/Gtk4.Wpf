namespace System.Windows.Automation;

/// <summary>The base of the client pattern classes: a wrapper over the provider a peer exposes.</summary>
public abstract class BasePattern
{
	internal BasePattern(AutomationElement element)
	{
		Element = element;
	}

	internal AutomationElement Element { get; }

	internal static InvalidOperationException NotCached() =>
		new InvalidOperationException("The pattern information was not cached.");

	/// <summary>Runs <paramref name="work"/> on the GTK thread; the element must still be available.</summary>
	internal T Run<T>(Func<T> work) => AutomationThread.Call(() =>
	{
		Element.EnsureAvailable();
		return work();
	});

	internal void Run(Action work) => Run(() =>
	{
		work();
		return true;
	});
}
