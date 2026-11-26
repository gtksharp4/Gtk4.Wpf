using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>Headless element for the pure mouse-input tests (never materialises a widget).</summary>
sealed class MouseTestElement : FrameworkElement
{
	public string Tag2 { get; }

	public MouseTestElement(string tag = "", UIElement? parent = null)
	{
		Tag2 = tag;
		if (parent != null) LogicalParent = parent;
	}

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure mouse tests.");
}

/// <summary>Headless Control that records the double-click virtuals.</summary>
sealed class MouseTestControl : Control
{
	public readonly List<string> Overrides = new List<string>();

	public MouseTestControl(UIElement? parent = null)
	{
		if (parent != null) LogicalParent = parent;
	}

	protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
	{
		Overrides.Add("OnPreviewMouseDoubleClick");
		base.OnPreviewMouseDoubleClick(e);
	}

	protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
	{
		Overrides.Add("OnMouseDoubleClick");
		base.OnMouseDoubleClick(e);
	}

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure mouse tests.");
}

/// <summary>Resets the static <see cref="Mouse"/> state around each test.</summary>
public abstract class MouseStateTestBase : IDisposable
{
	protected MouseStateTestBase() => Reset();

	public void Dispose() => Reset();

	static void Reset()
	{
		Mouse.Capture(null);
		Mouse.DirectlyOver = null;
		MouseDispatchDedupe.Reset();
		foreach (MouseButton b in Enum.GetValues(typeof(MouseButton)))
			Mouse.SetButtonState(b, MouseButtonState.Released);
	}

	protected static string Tag(object sender) => sender switch
	{
		MouseTestElement el => el.Tag2,
		MouseTestControl => "control",
		_ => sender.GetType().Name,
	};
}
