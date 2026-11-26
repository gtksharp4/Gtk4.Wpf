namespace System.Windows.Input;

/// <summary>
/// Identifies one GDK input event: its type, timestamp and surface position — the same for
/// every controller the event propagates through.
/// </summary>
internal readonly struct MouseEventKey : IEquatable<MouseEventKey>
{
	internal MouseEventKey(int type, uint time, double x, double y)
	{
		Type = type;
		Time = time;
		X = x;
		Y = y;
	}

	internal int Type { get; }
	internal uint Time { get; }
	internal double X { get; }
	internal double Y { get; }

	public bool Equals(MouseEventKey other) =>
		Type == other.Type && Time == other.Time && X.Equals(other.X) && Y.Equals(other.Y);

	public override bool Equals(object? obj) => obj is MouseEventKey k && Equals(k);

	// netstandard2.0-safe (no System.HashCode).
	public override int GetHashCode()
	{
		unchecked
		{
			int h = Type;
			h = (h * 397) ^ (int)Time;
			h = (h * 397) ^ X.GetHashCode();
			return (h * 397) ^ Y.GetHashCode();
		}
	}
}

/// <summary>
/// Single delivery of one GDK event. GTK propagates an unclaimed press/release/motion/scroll
/// through every ancestor's controllers (innermost first); the innermost element already ran the
/// whole WPF tunnel/bubble route, so each later controller seeing the same event must skip it —
/// unconditionally, as WPF raises each input report exactly once (a handler that captures the
/// mouse during MouseDown must not receive that MouseDown again from an ancestor).
/// </summary>
internal static class MouseDispatchDedupe
{
	static MouseEventKey? _last;

	/// <summary>
	/// Returns <see langword="true"/> when <paramref name="key"/> is the event dispatched last
	/// (a repeat delivery to an ancestor); otherwise records it and returns <see langword="false"/>.
	/// </summary>
	internal static bool IsDuplicate(MouseEventKey key)
	{
		if (_last.HasValue && _last.Value.Equals(key))
			return true;
		_last = key;
		return false;
	}

	/// <summary>Forgets the last dispatched event (test isolation).</summary>
	internal static void Reset() => _last = null;
}
