using System;
using System.Windows;
using System.Windows.Documents;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A minimal adorner that uses no GTK widget (pure object-model tests).</summary>
sealed class BoxAdorner : Adorner
{
	public BoxAdorner(UIElement adornedElement) : base(adornedElement) { }

	protected override Gtk.Widget CreateWidget() =>
		throw new NotSupportedException("No GTK in pure adorner tests.");
}
