using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// EventSetter handlers bound exactly as the compiler emits them (event delegate type + MethodInfo, so
/// contravariant handlers such as <c>(object, RoutedEventArgs)</c> on MouseDown bind like C# method groups).
/// </summary>
public class EventSetterDelegateTypeTests : WpfGtkTestBase
{
	sealed class Host
	{
		public int Downs;
		void OnDown(object s, MouseButtonEventArgs e) => Downs++;
		void OnDownBase(object s, RoutedEventArgs e) => Downs++;

		public Delegate Create(string name) =>
			Delegate.CreateDelegate(UIElement.MouseDownEvent.HandlerType, this,
				GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!);
	}

	[Theory]
	[InlineData("OnDown")]
	[InlineData("OnDownBase")]
	public void Private_handler_bound_with_CreateDelegate_runs_on_MouseDown(string handler) => Run(() =>
	{
		var host = new Host();
		var style = new Style(typeof(Border));
		style.Setters.Add(new EventSetter(UIElement.MouseDownEvent, host.Create(handler)));
		var border = new Border { Style = style };

		var args = new MouseButtonEventArgs(UIElement.MouseDownEvent, new Point(0, 0), MouseButton.Left);
		RoutedEventHost.RaiseEvent(border, args);

		Assert.Equal(1, host.Downs);
	});

	[Fact]
	public void Name_overload_rejects_a_contravariant_handler_which_is_why_the_compiler_uses_MethodInfo()
	{
		var host = new Host();
		Assert.Throws<ArgumentException>(() =>
			Delegate.CreateDelegate(UIElement.MouseDownEvent.HandlerType, host, "OnDownBase"));
	}
}
