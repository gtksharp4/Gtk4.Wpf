using System.Collections.Generic;

namespace System.Windows.Media.Animation;

public partial class DoubleAnimationUsingKeyFrames
{
	/// <summary>The largest TimeSpan key time, or one second when there is none.</summary>
	internal override Duration GetAutomaticDuration()
	{
		var longest = TimeSpan.Zero;
		bool any = false;
		foreach (var frame in _keyFrames ?? DoubleKeyFrameCollection.Empty)
		{
			var key = frame.KeyTime;
			if (key.Type != KeyTimeType.TimeSpan) continue;
			any = true;
			if (key.TimeSpan > longest) longest = key.TimeSpan;
		}
		return new Duration(any ? longest : TimeSpan.FromSeconds(1));
	}

	/// <inheritdoc/>
	protected sealed override double GetCurrentValueCore(
		double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
	{
		var frames = _keyFrames ?? DoubleKeyFrameCollection.Empty;
		if (frames.Count == 0) return defaultDestinationValue;

		var duration = ResolveDuration();
		double span = duration.HasTimeSpan ? duration.TimeSpan.TotalSeconds : 0;
		double now = (animationClock.CurrentProgress ?? 0) * span;
		var order = Resolve(frames, span, out var times);

		int index = 0;
		while (index < order.Count - 1 && times[index] < now) index++;

		double zero = IsAdditive ? 0 : defaultOriginValue;
		double value;
		if (times[index] <= now)
		{
			value = order[index].Value;
		}
		else
		{
			double from = index == 0 ? zero : order[index - 1].Value;
			double start = index == 0 ? 0 : times[index - 1];
			double length = times[index] - start;
			double progress = length <= 0 ? 1 : (now - start) / length;
			value = order[index].InterpolateValue(from, Math.Max(0, Math.Min(1, progress)));
		}

		int iteration = animationClock.CurrentIteration ?? 1;
		if (IsCumulative && iteration > 1)
			value += order[order.Count - 1].Value * (iteration - 1);
		return IsAdditive ? defaultOriginValue + value : value;
	}

	/// <summary>
	/// Resolves key times to seconds and orders the frames by them: time spans first, then percentages of the
	/// natural duration, then Uniform frames spread evenly between their resolved neighbours (a leading run starts
	/// from a virtual time 0). A leading Paced frame is pinned to 0; other Paced frames are treated
	/// as Uniform.
	/// </summary>
	static List<DoubleKeyFrame> Resolve(DoubleKeyFrameCollection frames, double span, out double[] times)
	{
		int n = frames.Count;
		var resolved = new double?[n];
		for (int i = 0; i < n; i++)
		{
			var key = frames[i].KeyTime;
			if (key.Type == KeyTimeType.TimeSpan) resolved[i] = key.TimeSpan.TotalSeconds;
			else if (key.Type == KeyTimeType.Percent) resolved[i] = key.Percent * span;
		}
		if (resolved[n - 1] == null)
		{
			double max = span;
			foreach (var r in resolved) if (r.HasValue && r.Value > max) max = r.Value;
			resolved[n - 1] = max;
		}
		if (resolved[0] == null && frames[0].KeyTime.Type == KeyTimeType.Paced) resolved[0] = 0;
		for (int i = 0; i < n - 1; i++)
		{
			if (resolved[i] != null) continue;
			int next = i;
			while (resolved[next] == null) next++;
			double startTime = i == 0 ? 0 : resolved[i - 1]!.Value, endTime = resolved[next]!.Value;
			int slots = next - (i - 1);
			for (int k = i; k < next; k++)
				resolved[k] = startTime + (endTime - startTime) * (k - (i - 1)) / slots;
		}

		var indices = new List<int>();
		for (int i = 0; i < n; i++) indices.Add(i);
		indices.Sort((a, b) =>
		{
			int c = resolved[a]!.Value.CompareTo(resolved[b]!.Value);
			return c != 0 ? c : a.CompareTo(b);
		});
		var order = new List<DoubleKeyFrame>(n);
		times = new double[n];
		for (int i = 0; i < n; i++)
		{
			order.Add(frames[indices[i]]);
			times[i] = resolved[indices[i]]!.Value;
		}
		return order;
	}
}
