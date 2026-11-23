using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Dictionary owner changes and name-change attribution.</summary>
public class ContextIndexOwnerTests : WpfGtkTestBase
{
	sealed class Probe : Border
	{
		public static readonly DependencyProperty SeenProperty = DependencyProperty.Register(
			nameof(Seen), typeof(string), typeof(Probe), new PropertyMetadata(null, (d, e) => OnSeen?.Invoke()));
		public static Action? OnSeen;
		public string? Seen
		{
			get => (string?)GetValue(SeenProperty);
			set => SetValue(SeenProperty, value);
		}
	}

	[Fact]
	public void A_CollectionViewSource_keeps_a_context_when_its_dictionary_changes_owner() => Run(() =>
	{
		var a = new Border();
		var b = new Border();
		var dict = new ResourceDictionary();
		dict.Owner = a;
		var cvs = new CollectionViewSource();
		dict["cvs"] = cvs;
		Assert.Same(a, cvs.InheritanceContext);
		dict.Owner = b;
		Assert.Same(b, cvs.InheritanceContext);
	});

	[Fact]
	public void A_Freezable_keeps_a_context_when_its_dictionary_changes_owner() => Run(() =>
	{
		var a = new Border();
		var b = new Border();
		var dict = new ResourceDictionary();
		dict.Owner = a;
		var brush = new SolidColorBrush(Colors.Red);
		dict["b"] = brush;
		dict.Owner = b;
		Assert.Same(b, brush.InheritanceContext);
	});

	[Fact]
	public void A_name_registered_by_another_scope_during_re_resolve_is_not_scoped_to_the_first_owner() => Run(() =>
	{
		var src = new Border { Tag = "x" };
		var root = new Border();
		var probe = new Probe();
		root.Child = probe;
		probe.SetBinding(Probe.SeenProperty, new Binding("Tag") { ElementName = "n1" });

		// A binding outside root that waits for a name another scope registers while root's change is re-resolving.
		var outside = new Probe();
		outside.SetBinding(Probe.SeenProperty, new Binding("Tag") { ElementName = "n2" });
		var other = new NameScope();
		NameScope.SetNameScope(outside, other);
		var n2 = new Border { Tag = "two" };
		Probe.OnSeen = () =>
		{
			Probe.OnSeen = null;
			other.RegisterName("n2", n2);
		};
		try
		{
			root.RegisterName("n1", src);
			Assert.Equal("two", outside.Seen);
		}
		finally { Probe.OnSeen = null; }
	});

	[Fact]
	public void A_Freezable_in_two_dictionaries_has_no_context_in_either_order() => Run(() =>
	{
		var first = new SolidColorBrush(Colors.Red);
		var a = new Border();
		var b = new Border();
		a.Resources["x"] = first;
		Assert.Same(a, first.InheritanceContext);
		b.Resources["x"] = first;
		Assert.Null(first.InheritanceContext);

		var second = new SolidColorBrush(Colors.Blue);
		var holder = new Border { Background = second };
		Assert.Same(holder, second.InheritanceContext);
		a.Resources["y"] = second;
		Assert.Null(second.InheritanceContext);
		holder.Background = null;
		Assert.Null(second.InheritanceContext);
	});
}
