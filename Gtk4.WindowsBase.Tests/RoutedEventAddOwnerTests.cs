using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class RoutedEventAddOwnerTests
{
	sealed class OwnerA { }
	sealed class OwnerB { }

	[Fact]
	public void AddOwner_returns_the_same_event_and_registers_the_new_owner()
	{
		var e = EventManager.RegisterRoutedEvent("AddOwnerProbe", RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(OwnerA));
		var same = e.AddOwner(typeof(OwnerB));
		Assert.Same(e, same);
		Assert.Same(e, EventManager.GetRoutedEvent(typeof(OwnerB), "AddOwnerProbe"));
		Assert.Equal(typeof(OwnerA), e.OwnerType);
	}

	[Fact]
	public void AddOwner_rejects_null()
	{
		var e = EventManager.RegisterRoutedEvent("AddOwnerNull", RoutingStrategy.Direct,
			typeof(RoutedEventHandler), typeof(OwnerA));
		Assert.Throws<ArgumentNullException>(() => e.AddOwner(null!));
	}
}
