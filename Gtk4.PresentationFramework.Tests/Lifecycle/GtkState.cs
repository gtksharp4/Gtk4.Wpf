using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Waits that fail with the live GTK state instead of a bare "Expected: True". macOS CI fails these
/// waits intermittently and only the failure message reaches the CI annotation, so it must say
/// whether the window was never mapped, mapped but not loaded, or loaded with no allocation.
/// </summary>
static class GtkState
{
	/// <summary>Pumps until <paramref name="condition"/> holds; otherwise fails with the elements' state.</summary>
	public static void Require(Func<bool> condition, string what, params FrameworkElement[] elements) =>
		Require(condition, () => what, elements);

	/// <summary>As above; <paramref name="what"/> is evaluated at failure time (e.g. to show an event log).</summary>
	public static void Require(Func<bool> condition, Func<string> what, params FrameworkElement[] elements)
	{
		// Generous: the wait returns as soon as the condition holds; slow CI runners (macOS) need the headroom.
		const int timeoutMs = 10000;
		if (!GtkPump.Until(condition, timeoutMs))
			Assert.Fail($"Timed out after {timeoutMs} ms waiting for {what()}. {Describe(elements)}");
	}

	/// <summary>One line per element: lifecycle, managed size and the backing widget's GTK state.</summary>
	public static string Describe(params FrameworkElement[] elements) =>
		string.Join(" | ", elements.Select(DescribeOne));

	static string DescribeOne(FrameworkElement e)
	{
		var inv = CultureInfo.InvariantCulture;
		var managed = string.Format(inv, "{0}: IsLoaded={1} Actual={2}x{3}",
			e.GetType().Name, e.IsLoaded, e.ActualWidth, e.ActualHeight);
		var w = e.Widget;
		var bounds = w.ComputeBounds(w, out var b)
			? string.Format(inv, "{0}x{1}", b.Width, b.Height)
			: "n/a";
		var gtk = string.Format(inv, "{0} Visible={1} Mapped={2} Content={3}x{4} Bounds={5}",
			w.GetType().Name, w.Visible, w.IsMapped, w.Width, w.Height, bounds);
		if (w is Gtk.Window gw)
			gtk += string.Format(inv, " Active={0}", gw.IsActive);
		return managed + "; " + gtk;
	}
}
