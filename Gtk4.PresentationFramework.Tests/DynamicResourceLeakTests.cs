using System;
using System.Runtime.CompilerServices;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Verifies that weak-event subscriptions in <see cref="ResourceDictionary.ResourceChanged"/>
/// do not pin subscriber elements, and that live elements still receive updates after a GC.
/// </summary>
public class DynamicResourceLeakTests
{
	sealed class El : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register(
				"LeakTestTag", typeof(string), typeof(El), new PropertyMetadata(null));

		public new string? Tag
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Leak test ──────────────────────────────────────────────────────────────

	/// <summary>
	/// An element that holds a SetResourceReference into a long-lived dictionary must
	/// become collectible once all strong references to the element are dropped.
	/// Without weak subscriptions the dict's strong delegate would keep it alive.
	/// </summary>
	[Fact]
	public void Collected_element_not_pinned_by_dictionary_subscription()
	{
		var dict = new ResourceDictionary();
		dict["color"] = "Red";

		WeakReference<El> weak = SubscribeAndDrop(dict);

		// Two full GC passes ensure the finalizer queue drains and Gen-2 is collected.
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
		GC.WaitForPendingFinalizers();
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

		Assert.False(weak.TryGetTarget(out _),
			"Element must be collectible after all strong refs are dropped; " +
			"a surviving WeakReference means the dict's subscription is still strong.");
	}

	/// <summary>
	/// Creates an element, subscribes it to <paramref name="dict"/>, and returns a
	/// <see cref="WeakReference{T}"/> to it.  Isolated in a no-inline method so the
	/// JIT does not leave a stack-frame root pointing at the element after the method
	/// returns, which would defeat the GC test.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference<El> SubscribeAndDrop(ResourceDictionary dict)
	{
		var el = new El();
		el.SetResourceReference(El.TagProperty, "color");

		// el.Resources is the element's own dict; we also need to subscribe it to `dict`
		// (the long-lived dict) so the subscription crosses the element's lifetime.
		// Add `dict` as a merged dictionary on the element's own resources so
		// CollectResourceChain picks it up.
		el.Resources.MergedDictionaries.Add(dict);
		el.SetResourceReference(El.TagProperty, "color");  // re-subscribe with new chain

		var weak = new WeakReference<El>(el);
		// el goes out of scope here; no strong ref survives the method.
		return weak;
	}

	// ── Live-update-after-GC regression ───────────────────────────────────────

	/// <summary>
	/// A still-reachable element must continue to receive live updates after a GC cycle
	/// (i.e., a GC that may prune dead entries does not accidentally prune live ones).
	/// </summary>
	[Fact]
	public void Live_element_still_receives_updates_after_GC()
	{
		var dict = new ResourceDictionary();
		dict["color"] = "Red";

		var el = new El();
		el.Resources.MergedDictionaries.Add(dict);
		el.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Red", el.Tag);

		// Force a GC while `el` is still reachable.
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
		GC.WaitForPendingFinalizers();
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

		// Live update must still work.
		dict["color"] = "Blue";

		Assert.Equal("Blue", el.Tag);

		GC.KeepAlive(el);  // ensure el is not collected before the assertion above
	}
}
