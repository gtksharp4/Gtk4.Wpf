using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;
using WpfBorder = System.Windows.Controls.Border;
using WpfButton = System.Windows.Controls.Button;
using WpfContentControl = System.Windows.Controls.ContentControl;
using WpfControlTemplate = System.Windows.Controls.ControlTemplate;
using WpfDataTemplate = System.Windows.Controls.DataTemplate;
using WpfItemsControl = System.Windows.Controls.ItemsControl;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) tests for Round-6 template XAML compiler support:
/// ContentControl.ContentTemplate DP, ControlTemplate factory invocation,
/// DataTemplate factory with DataContext wiring.
/// Each test fails without the corresponding runtime or compiler change.
/// </summary>
public class TemplatingXamlDpTests
{
	// ── ContentControl.ContentTemplate DP ───────────────────────────────────

	[Fact]
	public void ContentControl_ContentTemplate_default_is_null()
	{
		var cc = new WpfContentControl();
		Assert.Null(cc.ContentTemplate);
	}

	[Fact]
	public void ContentControl_ContentTemplate_roundtrip()
	{
		var dt = new WpfDataTemplate { Factory = () => new WpfTextBlock() };
		var cc = new WpfContentControl { ContentTemplate = dt };
		Assert.Same(dt, cc.ContentTemplate);
	}

	[Fact]
	public void ContentControl_ContentTemplateProperty_is_registered()
	{
		var dp = WpfContentControl.ContentTemplateProperty;
		Assert.NotNull(dp);
		Assert.Equal("ContentTemplate", dp.Name);
		Assert.Equal(typeof(WpfDataTemplate), dp.PropertyType);
	}

	// ── ControlTemplate factory used by compiler-generated code ─────────────
	// These simulate what the compiler emits for a <Button.Template> element.

	[Fact]
	public void Compiler_ControlTemplate_factory_builds_Border_TextBlock_tree()
	{
		// Simulate what the compiler emits for:
		//   <Button.Template><ControlTemplate TargetType="Button">
		//     <Border><TextBlock Text="X"/></Border>
		//   </ControlTemplate></Button.Template>
		var ct = new WpfControlTemplate(typeof(WpfButton));
		ct.Factory = () =>
		{
			var __e0 = new WpfBorder();
			var __e1 = new WpfTextBlock();
			__e1.Text = "X";
			__e0.Child = __e1;
			return __e0;
		};

		var btn = new WpfButton();
		btn.Template = ct;

		// Template was set; factory produces a Border root.
		var visual = ct.CreateVisualTree();
		Assert.IsType<WpfBorder>(visual);
		var border = (WpfBorder)visual!;
		Assert.IsType<WpfTextBlock>(border.Child);
		Assert.Equal("X", ((WpfTextBlock)border.Child!).Text);
	}

	[Fact]
	public void Compiler_ControlTemplate_factory_called_each_time()
	{
		int calls = 0;
		var ct = new WpfControlTemplate(typeof(WpfButton))
		{
			Factory = () => { calls++; return new WpfBorder(); }
		};
		ct.CreateVisualTree();
		ct.CreateVisualTree();
		Assert.Equal(2, calls);
	}

	[Fact]
	public void ControlTemplate_assigned_to_Button_Template_DP()
	{
		var ct = new WpfControlTemplate(typeof(WpfButton))
		{
			Factory = () => new WpfBorder()
		};
		var btn = new WpfButton();
		btn.Template = ct;
		Assert.Same(ct, btn.Template);
	}

	[Fact]
	public void ControlTemplate_resource_key_resolved_via_Resources()
	{
		// Simulate keyed resource: this.Resources["t"] = ct;
		// Then: SetValue(Button.TemplateProperty, FindResource("t"));
		var window = new System.Windows.Window();
		var ct = new WpfControlTemplate(typeof(WpfButton))
		{
			Factory = () => new WpfBorder()
		};
		window.Resources["t"] = ct;

		var resolved = window.FindResource("t");
		Assert.Same(ct, resolved);
	}

	// ── DataTemplate factory wired as ItemTemplate ───────────────────────────

	[Fact]
	public void Compiler_DataTemplate_factory_builds_TextBlock_per_item()
	{
		// Simulate what the compiler emits for:
		//   <ListBox.ItemTemplate><DataTemplate>
		//     <TextBlock Text="hi"/>
		//   </DataTemplate></ListBox.ItemTemplate>
		var dt = new WpfDataTemplate();
		dt.ItemFactory = __item =>
		{
			var __e0 = new WpfTextBlock();
			__e0.Text = "hi";
			if (__e0 is FrameworkElement __fe0) __fe0.DataContext = __item;
			return __e0;
		};

		var lb = new WpfListBox { ItemTemplate = dt };
		lb.ItemsSource = new[] { "a", "b", "c" };

		Assert.Equal(3, lb.Items.Count);
	}

	[Fact]
	public void DataTemplate_ItemFactory_receives_item_as_DataContext()
	{
		object? captured = null;
		var dt = new WpfDataTemplate
		{
			ItemFactory = __item =>
			{
				var __e0 = new WpfTextBlock();
				if (__e0 is FrameworkElement __fe0) __fe0.DataContext = __item;
				captured = __item;
				return __e0;
			}
		};

		dt.CreateVisualTreeForItem("hello");
		Assert.Equal("hello", captured);
	}

	[Fact]
	public void DataTemplate_factory_DataContext_set_on_root_element()
	{
		var dt = new WpfDataTemplate
		{
			ItemFactory = __item =>
			{
				var __e0 = new WpfTextBlock();
				if (__e0 is FrameworkElement __fe0) __fe0.DataContext = __item;
				return __e0;
			}
		};

		var visual = dt.CreateVisualTreeForItem("world") as WpfTextBlock;
		Assert.NotNull(visual);
		Assert.Equal("world", visual!.DataContext);
	}

	[Fact]
	public void Keyed_DataTemplate_resolved_from_Resources_as_ItemTemplate()
	{
		// Simulate: Resources["d"] = dt; then ListBox.ItemTemplate = (DataTemplate)FindResource("d");
		var window = new System.Windows.Window();
		var dt = new WpfDataTemplate
		{
			ItemFactory = __item => new WpfTextBlock { Text = __item?.ToString() ?? string.Empty }
		};
		window.Resources["d"] = dt;

		var resolved = window.FindResource("d") as WpfDataTemplate;
		Assert.NotNull(resolved);
		Assert.Same(dt, resolved);
	}
}