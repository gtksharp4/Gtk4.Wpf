using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>A non-<see cref="RoutedEventHandler"/> handler type (invoked via DynamicInvoke).</summary>
delegate void CustomArgsHandler(object sender, RoutedEventArgs e);

/// <summary>
/// A handler's own exception propagates out of <c>RaiseEvent</c> unchanged — not wrapped in
/// <see cref="System.Reflection.TargetInvocationException"/> (WPF invokes handlers typed).
/// </summary>
public class RoutedEventHandlerExceptionTests
{
	static int _counter;

	static RoutedEvent NewEvent(Type handlerType) =>
		EventManager.RegisterRoutedEvent(
			"Ex" + System.Threading.Interlocked.Increment(ref _counter), RoutingStrategy.Bubble,
			handlerType, typeof(ChBase));

	[Fact]
	public void RoutedEventHandler_exception_propagates_unwrapped()
	{
		var ev = NewEvent(typeof(RoutedEventHandler));
		var el = new ChBase();
		el.AddHandler(ev, new RoutedEventHandler((_, _) => throw new InvalidOperationException("boom")));

		var ex = Assert.Throws<InvalidOperationException>(() => el.RaiseEvent(new RoutedEventArgs(ev)));
		Assert.Equal("boom", ex.Message);
	}

	[Fact]
	public void Custom_handler_type_exception_propagates_unwrapped()
	{
		var ev = NewEvent(typeof(CustomArgsHandler));
		var el = new ChBase();
		el.AddHandler(ev, new CustomArgsHandler((_, _) => throw new ArgumentException("bad")));

		var ex = Assert.Throws<ArgumentException>(() => el.RaiseEvent(new RoutedEventArgs(ev)));
		Assert.Equal("bad", ex.Message);
	}

	[Fact]
	public void Exception_from_an_event_raised_inside_a_class_handler_propagates_unwrapped()
	{
		// The button-cracking shape: a class handler re-raises another event whose instance
		// handler throws — two DynamicInvoke levels, still the original exception.
		var outer = NewEvent(typeof(CustomArgsHandler));
		var inner = NewEvent(typeof(CustomArgsHandler));
		EventManager.RegisterClassHandler(typeof(ChBase), outer,
			new CustomArgsHandler((s, _) => ((ChBase)s).RaiseEvent(new RoutedEventArgs(inner))));
		var el = new ChBase();
		el.AddHandler(inner, new CustomArgsHandler((_, _) => throw new FormatException("nested")));

		var ex = Assert.Throws<FormatException>(() => el.RaiseEvent(new RoutedEventArgs(outer)));
		Assert.Equal("nested", ex.Message);
	}
}
