using System;
using System.Windows;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Shared stubs for the AutomationProperties tests.</summary>
public abstract class AutomationTestBase
{
	/// <summary>UIElement that never materialises a GTK widget.</summary>
	protected sealed class StubElement : UIElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}

	/// <summary>Non-UIElement DependencyObject for attached-DP semantics tests.</summary>
	protected sealed class PlainDO : DependencyObject { }
}
