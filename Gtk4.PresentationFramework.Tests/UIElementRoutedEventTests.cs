using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="UIElement"/> routed-event surface, covering the
/// WPF-compatible 3-arg <c>AddHandler</c> overload (#6).
/// </summary>
public class UIElementRoutedEventTests
{
	// ── Domain types ───────────────────────────────────────────────────────────

	sealed class TestElement : UIElement
	{
		public new UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		public static readonly RoutedEvent ClickEvent =
			EventManager.RegisterRoutedEvent(
				"UIEClick", RoutingStrategy.Bubble,
				typeof(RoutedEventHandler), typeof(TestElement));

		public void FireClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));

		protected override Gtk.Widget CreateWidget() =>
			throw new System.NotSupportedException("No GTK in pure tests.");
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

	/// <summary>#6: 3-arg AddHandler compiles and fires the handler.</summary>
	[Fact]
	public void AddHandler_3arg_fires_handler()
	{
		var el = new TestElement();
		bool fired = false;
		el.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => fired = true),
			handledEventsToo: false);

		el.FireClick();

		Assert.True(fired);
	}

	/// <summary>#6: 3-arg AddHandler with handledEventsToo=true receives already-handled events.</summary>
	[Fact]
	public void AddHandler_3arg_handledEventsToo_fires_after_handled()
	{
		var parent = new TestElement();
		var child = new TestElement { LogicalParent = parent };
		bool parentFired = false;

		child.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => e.Handled = true),
			handledEventsToo: false);
		parent.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => parentFired = true),
			handledEventsToo: true);

		child.FireClick();

		Assert.True(parentFired);
	}

	/// <summary>#6: 3-arg AddHandler with handledEventsToo=false does not receive already-handled events.</summary>
	[Fact]
	public void AddHandler_3arg_no_handledEventsToo_blocked_after_handled()
	{
		var parent = new TestElement();
		var child = new TestElement { LogicalParent = parent };
		bool parentFired = false;

		child.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => e.Handled = true),
			handledEventsToo: false);
		parent.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => parentFired = true),
			handledEventsToo: false);

		child.FireClick();

		Assert.False(parentFired);
	}

	/// <summary>#6: 2-arg AddHandler (existing overload) still works.</summary>
	[Fact]
	public void AddHandler_2arg_fires_handler()
	{
		var el = new TestElement();
		bool fired = false;
		el.AddHandler(TestElement.ClickEvent,
			new RoutedEventHandler((s, e) => fired = true));

		el.FireClick();

		Assert.True(fired);
	}

	/// <summary>#6: RemoveHandler removes a 3-arg-registered handler.</summary>
	[Fact]
	public void RemoveHandler_removes_3arg_registered_handler()
	{
		var el = new TestElement();
		int count = 0;
		RoutedEventHandler h = (s, e) => count++;
		el.AddHandler(TestElement.ClickEvent, h, handledEventsToo: false);
		el.RemoveHandler(TestElement.ClickEvent, h);

		el.FireClick();

		Assert.Equal(0, count);
	}
}