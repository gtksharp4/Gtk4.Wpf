using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>SetResourceReference over a XAML-local reference is one A -> B change.</summary>
public class Round51aResourceReferenceTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	sealed class Pane : FrameworkElement
	{
		public readonly List<(object? Old, object? New)> Changes = new List<(object?, object?)>();

		public static readonly DependencyProperty AProperty = DependencyProperty.Register(
			"A51", typeof(object), typeof(Pane), new PropertyMetadata("a0", (d, e) =>
				((Pane)d).Changes.Add((e.OldValue, e.NewValue))));

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Replacing_a_local_reference_raises_a_single_change() => Run(() =>
	{
		var pane = new Pane();
		pane.Resources["k1"] = "A";
		pane.Resources["k2"] = "B";
		pane.SetLocalResourceReference(Pane.AProperty, "k1");
		pane.Changes.Clear();

		pane.SetResourceReference(Pane.AProperty, "k2");

		Assert.Equal(new (object?, object?)[] { ("A", "B") }, pane.Changes.ToArray());
		Assert.Equal("B", pane.GetValue(Pane.AProperty));
		pane.Resources["k2"] = "C";
		Assert.Equal("C", pane.GetValue(Pane.AProperty));
	});

	[Fact]
	public void Unresolved_new_key_leaves_the_default() => Run(() =>
	{
		var pane = new Pane();
		pane.Resources["k1"] = "A";
		pane.SetLocalResourceReference(Pane.AProperty, "k1");

		pane.SetResourceReference(Pane.AProperty, "missing");

		Assert.Equal("a0", pane.GetValue(Pane.AProperty));
	});
}
