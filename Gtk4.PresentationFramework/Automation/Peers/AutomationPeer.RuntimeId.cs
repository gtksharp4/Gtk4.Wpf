using System.Threading;

namespace System.Windows.Automation.Peers;

public abstract partial class AutomationPeer
{
	static int _nextRuntimeId;
	int _runtimeId;

	/// <summary>The id that tells this peer apart from every other for the life of the process.</summary>
	internal int RuntimeId
	{
		get
		{
			if (_runtimeId == 0)
				Interlocked.CompareExchange(ref _runtimeId, Interlocked.Increment(ref _nextRuntimeId), 0);
			return _runtimeId;
		}
	}
}
