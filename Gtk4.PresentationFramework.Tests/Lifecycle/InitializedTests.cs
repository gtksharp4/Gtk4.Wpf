using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Pure tests for <see cref="FrameworkElement.Initialized"/>, <see cref="FrameworkElement.IsInitialized"/>
/// and the <see cref="ISupportInitialize"/> protocol.
/// </summary>
public class InitializedTests : IDisposable
{
	// Several tests start an init without ending it; drop the thread-wide scope so later tests are unaffected.
	public void Dispose() => InitScope.Reset();

	sealed class TrackingPanel : StackPanel
	{
		public readonly List<string> Calls = new List<string>();

		protected override void OnInitialized(EventArgs e)
		{
			Calls.Add("OnInitialized:" + IsInitialized);
			base.OnInitialized(e);
		}
	}

	[Fact]
	public void FrameworkElement_implements_ISupportInitialize()
	{
		Assert.IsAssignableFrom<ISupportInitialize>(new Border());
		Assert.False(new Border().IsInitialized);
	}

	[Fact]
	public void BeginInit_EndInit_fires_once_and_IsInitialized_flips_before_handler()
	{
		var b = new Border();
		int count = 0;
		bool? seen = null;
		b.Initialized += (s, e) => { count++; seen = b.IsInitialized; Assert.Same(b, s); };

		b.BeginInit();
		Assert.Equal(0, count);
		b.EndInit();

		Assert.Equal(1, count);
		Assert.True(seen);
		Assert.True(b.IsInitialized);

		// A later init cycle does not re-raise.
		b.BeginInit();
		b.EndInit();
		Assert.Equal(1, count);
	}

	[Fact]
	public void Nested_BeginInit_throws_InvalidOperationException()
	{
		var b = new Border();
		b.BeginInit();
		Assert.Throws<InvalidOperationException>(() => b.BeginInit());
	}

	[Fact]
	public void EndInit_without_BeginInit_throws_InvalidOperationException()
	{
		var b = new Border();
		Assert.Throws<InvalidOperationException>(() => b.EndInit());
	}

	[Fact]
	public void Element_without_BeginInit_is_initialized_when_attached_to_parent()
	{
		var parent = new StackPanel();
		var child = new Border();
		int count = 0;
		child.Initialized += (_, _) => count++;

		parent.Children.Add(child);

		Assert.Equal(1, count);
		Assert.True(child.IsInitialized);
	}

	[Fact]
	public void Element_inside_BeginInit_is_not_initialized_on_attach_until_EndInit()
	{
		var parent = new StackPanel();
		var child = new Border();
		child.BeginInit();

		parent.Children.Add(child);
		Assert.False(child.IsInitialized);

		child.EndInit();
		Assert.True(child.IsInitialized);
	}

	[Fact]
	public void Child_is_initialized_before_parent_EndInit_completes()
	{
		var order = new List<string>();
		var parent = new StackPanel();
		var child = new Border();
		parent.Initialized += (_, _) => order.Add("parent");
		child.Initialized += (_, _) => order.Add("child");

		parent.BeginInit();
		parent.Children.Add(child);
		parent.EndInit();

		Assert.Equal(new[] { "child", "parent" }, order);
	}

	[Fact]
	public void OnInitialized_override_is_called_after_IsInitialized_set()
	{
		var p = new TrackingPanel();
		p.BeginInit();
		p.EndInit();

		Assert.Equal(new[] { "OnInitialized:True" }, p.Calls);
	}

	[Fact]
	public void Loaded_broadcast_initializes_uninitialized_root_first()
	{
		var root = new StackPanel();
		var order = new List<string>();
		root.Initialized += (_, _) => order.Add("Initialized");
		root.Loaded += (_, _) => order.Add("Loaded");

		LifecycleBroadcaster.BroadcastLoaded(root);

		Assert.Equal(new[] { "Initialized", "Loaded" }, order);
	}
}
