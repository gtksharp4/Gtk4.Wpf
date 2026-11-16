using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media.Animation
{
	/// <summary>
	/// Groups a set of <see cref="AnimationTimeline"/> children and applies them to named
	/// properties on named target elements.  Mirrors WPF's
	/// <c>System.Windows.Media.Animation.Storyboard</c>.
	///
	/// Target resolution: each child timeline may have a target DP set via
	/// <see cref="SetTargetProperty"/>.  The target element is the <c>target</c> argument
	/// passed to <see cref="Begin(FrameworkElement)"/> unless overridden per-child by
	/// <see cref="SetTargetName"/>.
	/// </summary>
	public class Storyboard : Timeline
	{
		// ── Children collection ──────────────────────────────────────────────────

		/// <summary>The timeline children owned by this storyboard.</summary>
		public TimelineCollection Children { get; } = new TimelineCollection();

		// ── Attached-property metadata ──────────────────────────────────────────
		// Storyboard uses plain per-instance dictionaries (not real DPs) for the
		// XAML-style SetTarget/SetTargetName/SetTargetProperty helpers.

		readonly Dictionary<Timeline, string> _targetNames =
			new Dictionary<Timeline, string>(System.Collections.Generic.ReferenceEqualityComparer.Instance);

		readonly Dictionary<Timeline, DependencyProperty> _targetProperties =
			new Dictionary<Timeline, DependencyProperty>(System.Collections.Generic.ReferenceEqualityComparer.Instance);

		/// <summary>Sets the target element name for a child timeline.</summary>
		public void SetTargetName(Timeline timeline, string name)
		{
			if (timeline == null) throw new ArgumentNullException(nameof(timeline));
			_targetNames[timeline] = name;
		}

		/// <summary>Returns the target element name for <paramref name="timeline"/>, or null.</summary>
		public string? GetTargetName(Timeline timeline) =>
			timeline != null && _targetNames.TryGetValue(timeline, out var n) ? n : null;

		/// <summary>Sets the target <see cref="DependencyProperty"/> for a child timeline.</summary>
		public void SetTargetProperty(Timeline timeline, DependencyProperty dp)
		{
			if (timeline == null) throw new ArgumentNullException(nameof(timeline));
			if (dp == null) throw new ArgumentNullException(nameof(dp));
			_targetProperties[timeline] = dp;
		}

		/// <summary>Returns the target DP for <paramref name="timeline"/>, or null.</summary>
		public DependencyProperty? GetTargetProperty(Timeline timeline) =>
			timeline != null && _targetProperties.TryGetValue(timeline, out var dp) ? dp : null;

		// ── Static helpers (WPF attached-property API style) ─────────────────────

		/// <summary>
		/// Sets the target element name on <paramref name="timeline"/> (attached-property style).
		/// </summary>
		public static void SetTargetName(Timeline timeline, Storyboard storyboard, string name) =>
			storyboard.SetTargetName(timeline, name);

		/// <summary>
		/// Sets the target property on <paramref name="timeline"/> (attached-property style).
		/// </summary>
		public static void SetTargetProperty(Timeline timeline, Storyboard storyboard, DependencyProperty dp) =>
			storyboard.SetTargetProperty(timeline, dp);

		// ── Playback ─────────────────────────────────────────────────────────────

		readonly List<AnimationClock> _clocks = new List<AnimationClock>();

		/// <summary>
		/// Starts all child timelines against <paramref name="target"/>.
		/// Each child that has a registered target DP gets an <see cref="AnimationClock"/>
		/// that is ticked by the <see cref="AnimationScheduler"/> (or directly in tests).
		/// </summary>
		public void Begin(FrameworkElement target)
		{
			Stop();
			foreach (var child in Children)
			{
				if (child is AnimationTimeline anim)
				{
					var dp = GetTargetProperty(child);
					if (dp == null) continue;

					// Target element: resolve by name or fall back to the storyboard target.
					DependencyObject animTarget = target;
					var nameStr = GetTargetName(child);
					if (!string.IsNullOrEmpty(nameStr))
					{
						var named = target.FindName(nameStr) as DependencyObject;
						if (named != null) animTarget = named;
					}

					var clock = new AnimationClock(anim, animTarget, dp);
					_clocks.Add(clock);
					AnimationScheduler.Register(clock);
				}
			}
		}

		/// <summary>Stops all clocks and clears animation values.</summary>
		public void Stop()
		{
			foreach (var c in _clocks)
			{
				AnimationScheduler.Unregister(c);
				c.Stop();
			}
			_clocks.Clear();
		}

		/// <summary>Pauses all clocks.</summary>
		public void Pause()
		{
			foreach (var c in _clocks)
				c.Pause();
		}

		/// <summary>Resumes all paused clocks.</summary>
		public void Resume()
		{
			foreach (var c in _clocks)
				c.Resume();
		}

		/// <summary>
		/// Returns the clocks started by the most recent <see cref="Begin(FrameworkElement)"/>
		/// call. Used by unit tests to tick the clocks deterministically without GLib.
		/// </summary>
		internal IReadOnlyList<AnimationClock> ActiveClocks => _clocks.AsReadOnly();
	}

	/// <summary>Collection of <see cref="Timeline"/> children.</summary>
	public class TimelineCollection : Collection<Timeline> { }
}
