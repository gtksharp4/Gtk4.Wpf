using System.Diagnostics;

namespace System.Windows.Media.Animation;

public partial class Storyboard
{
	/// <summary>Seeks every controllable run of this storyboard.</summary>
	public void Seek(TimeSpan offset, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime)
	{
		foreach (var run in _runs.Values)
			if (run.Controllable) run.Root.Controller!.Seek(offset, origin);
			else TraceNotControllable(nameof(Seek));
	}

	/// <summary>Seeks every controllable run on the last tick.</summary>
	public void SeekAlignedToLastTick(TimeSpan offset, TimeSeekOrigin origin = TimeSeekOrigin.BeginTime)
	{
		foreach (var run in _runs.Values)
			if (run.Controllable) run.Root.Controller!.SeekAlignedToLastTick(offset, origin);
			else TraceNotControllable(nameof(SeekAlignedToLastTick));
	}

	static void TraceNotControllable(string action) =>
		Trace.WriteLine($"Storyboard.{action} ignored: the storyboard was not begun with isControllable = true.");
}
