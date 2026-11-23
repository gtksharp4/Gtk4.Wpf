using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>The listener index must not leak dead targets, re-resolve per scope, or allocate for a miss.</summary>
public class ContextIndexHygieneTests : WpfGtkTestBase
{
	static void BuildAndDrop(int count)
	{
		for (int i = 0; i < count; i++)
		{
			var bound = new TextBlock();
			bound.SetBinding(TextBlock.TextProperty, new Binding("Text") { ElementName = "leakHead" });
		}
	}

	[Fact]
	public void Dropped_ElementName_targets_do_not_stay_registered() => Run(() =>
	{
		BuildAndDrop(50);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		BindingEngine.NameScopeChanged("leakHead");
		Assert.True(BindingEngine.NamedRegistrations("leakHead") < 5,
			$"{BindingEngine.NamedRegistrations("leakHead")} registrations left");
	});

	[Fact]
	public void A_new_name_scope_re_resolves_only_the_listeners_below_it() => Run(() =>
	{
		var anchor = new TextBlock();
		anchor.SetBinding(TextBlock.TextProperty, new Binding("Text") { ElementName = "nobody" });
		var keep = new Border[40];
		for (int i = 0; i < keep.Length; i++)
		{
			var item = keep[i] = new Border();
			var bound = new TextBlock();
			bound.SetBinding(TextBlock.TextProperty, new Binding("Text") { ElementName = "itemHead" });
			item.Child = bound;
		}
		var before = BindingEngine.ReResolveCalls;
		foreach (var item in keep) item.RegisterName("itemHead", new TextBlock { Text = "x" });
		var calls = BindingEngine.ReResolveCalls - before;
		GC.KeepAlive(anchor);
		Assert.True(calls <= keep.Length * 4, $"{calls} re-resolves for {keep.Length} scopes");
	});

	[Fact]
	public void A_tree_change_with_no_listener_below_allocates_nothing() => Run(() =>
	{
		var anchor = new TextBlock();
		anchor.SetBinding(TextBlock.TextProperty, new Binding("Text") { ElementName = "elsewhere" });
		var panel = new StackPanel();
		panel.Children.Add(new TextBlock());
		var leaf = new TextBlock();
		BindingEngine.ListenersUnderForTest(panel);
		BindingEngine.ListenersUnderForTest(leaf);
		var before = GC.GetAllocatedBytesForCurrentThread();
		BindingEngine.ListenersUnderForTest(panel);
		BindingEngine.ListenersUnderForTest(leaf);
		var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
		GC.KeepAlive(anchor);
		Assert.Equal(0, bytes);
	});

	[Fact]
	public void A_freezable_held_locally_then_added_to_another_elements_dictionary_has_no_context() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Blue);
		var holder = new Border { Background = brush };
		var border = new Border();
		border.Resources["b"] = brush;
		Assert.Null(brush.InheritanceContext);
	});

	[Fact]
	public void A_freezable_in_a_dictionary_then_held_locally_has_no_context() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Blue);
		var border = new Border();
		border.Resources["b"] = brush;
		Assert.Same(border, brush.InheritanceContext);
		var holder = new Border { Background = brush };
		GC.KeepAlive(holder);
		Assert.Null(brush.InheritanceContext);
	});
}
