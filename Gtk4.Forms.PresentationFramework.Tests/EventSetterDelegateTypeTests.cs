using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>EventSetter handlers use the event's own delegate type, as the compiler now emits.</summary>
public class EventSetterDelegateTypeTests : WpfGtkTestBase
{
	sealed class Host
	{
		public int Downs;
		void OnDown(object s, MouseButtonEventArgs e) => Downs++;

		public Delegate Create() =>
			Delegate.CreateDelegate(UIElement.MouseDownEvent.HandlerType, this, "OnDown");
	}

	[Fact]
	public void Private_MouseButtonEventHandler_bound_with_CreateDelegate_runs_on_MouseDown() => Run(() =>
	{
		var host = new Host();
		var style = new Style(typeof(Border));
		style.Setters.Add(new EventSetter(UIElement.MouseDownEvent, host.Create()));
		var border = new Border { Style = style };

		var args = new MouseButtonEventArgs(UIElement.MouseDownEvent, new Point(0, 0), MouseButton.Left);
		RoutedEventHost.RaiseEvent(border, args);

		Assert.Equal(1, host.Downs);
	});
}
