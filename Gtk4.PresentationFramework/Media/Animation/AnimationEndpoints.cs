namespace System.Windows.Media.Animation;

/// <summary>Resolves the From/To/By triple of a from-to-by animation against its defaults.</summary>
internal static class AnimationEndpoints
{
	internal static void Resolve<T>(
		T? from, T? to, T? by, T origin, T destination, Func<T, T, T> add, out T start, out T end)
		where T : struct
	{
		start = from ?? origin;
		if (to.HasValue) end = to.Value;
		else if (by.HasValue) end = add(start, by.Value);
		else end = destination;
	}

	/// <summary>Eases a progress value, or passes it through when there is no easing function.</summary>
	internal static double Progress(AnimationClock clock, IEasingFunction? easing)
	{
		var t = clock.CurrentProgress ?? 0;
		return easing == null ? t : easing.Ease(t);
	}
}
