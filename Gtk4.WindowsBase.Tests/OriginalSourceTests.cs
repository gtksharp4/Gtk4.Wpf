using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

class OriginalSourceElement : DependencyObject
{
	public DependencyObject? Parent { get => InheritanceParent; set => InheritanceParent = value; }

	public static readonly RoutedEvent PingEvent = EventManager.RegisterRoutedEvent(
		"Ping", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OriginalSourceElement));
}

class SourceTrackingArgs : RoutedEventArgs
{
	public int SetCount;
	public SourceTrackingArgs(RoutedEvent e) : base(e) { }
	protected override void OnSetSource(object source) => SetCount++;
}

public class OriginalSourceTests
{
	[Fact]
	public void First_Source_assignment_sets_both()
	{
		var a = new RoutedEventArgs(OriginalSourceElement.PingEvent);
		var src = new object();
		a.Source = src;
		Assert.Same(src, a.Source);
		Assert.Same(src, a.OriginalSource);
	}

	[Fact]
	public void Later_Source_change_keeps_OriginalSource()
	{
		var a = new RoutedEventArgs(OriginalSourceElement.PingEvent);
		object first = new object(), second = new object();
		a.Source = first;
		a.Source = second;
		Assert.Same(second, a.Source);
		Assert.Same(first, a.OriginalSource);
	}

	[Fact]
	public void Ctor_with_source_sets_both_and_calls_OnSetSource_for_changes_only()
	{
		var src = new object();
		var a = new RoutedEventArgs(OriginalSourceElement.PingEvent, src);
		Assert.Same(src, a.OriginalSource);
		var t = new SourceTrackingArgs(OriginalSourceElement.PingEvent);
		t.Source = src;
		t.Source = src;
		t.Source = new object();
		Assert.Equal(2, t.SetCount);
	}

	[Fact]
	public void RaiseEvent_fills_both_from_the_raising_element_and_handler_change_keeps_original()
	{
		var parent = new OriginalSourceElement();
		var child = new OriginalSourceElement { Parent = parent };
		object? seenOriginal = null, seenSource = null;
		child.AddHandler(OriginalSourceElement.PingEvent, new RoutedEventHandler((s, e) => e.Source = parent));
		parent.AddHandler(OriginalSourceElement.PingEvent, new RoutedEventHandler((s, e) =>
		{
			seenOriginal = e.OriginalSource;
			seenSource = e.Source;
		}));

		child.RaiseEvent(new RoutedEventArgs(OriginalSourceElement.PingEvent));

		Assert.Same(child, seenOriginal);
		Assert.Same(parent, seenSource);
	}

	[Fact]
	public void Event_args_without_event_have_no_source()
	{
		var a = new RoutedEventArgs();
		Assert.Null(a.Source);
		Assert.Null(a.OriginalSource);
	}
}
