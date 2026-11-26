using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Pure tests for <see cref="FrameworkElement.ActualWidth"/>/<see cref="FrameworkElement.ActualHeight"/>,
/// <see cref="FrameworkElement.SizeChanged"/>, <see cref="SizeChangedInfo"/> and
/// <see cref="SizeChangedEventArgs"/>, driven by the managed Arrange pass.
/// </summary>
public class SizeChangedTests
{
	sealed class OrderBorder : Border
	{
		public readonly List<string> Log = new List<string>();

		protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			Log.Add("before-base:" + ActualWidth + "/" + GetValue(ActualWidthProperty));
			base.OnRenderSizeChanged(sizeInfo);
			Log.Add("after-base:" + GetValue(ActualWidthProperty));
		}
	}

	sealed class Sink : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("LifecycleSinkValue", typeof(object), typeof(Sink), new PropertyMetadata(null));

		public object? Value => GetValue(ValueProperty);

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	static void Layout(UIElement el, double w, double h, double x = 0)
	{
		el.Measure(new Size(w, h));
		el.Arrange(new Rect(x, 0, w, h));
	}

	[Fact]
	public void Arrange_to_new_size_raises_SizeChanged_with_correct_args()
	{
		var b = new Border();
		var events = new List<SizeChangedEventArgs>();
		b.SizeChanged += (_, e) => events.Add(e);

		Layout(b, 100, 50);
		Layout(b, 100, 80);

		Assert.Equal(2, events.Count);
		Assert.Equal(new Size(0, 0), events[0].PreviousSize);
		Assert.Equal(new Size(100, 50), events[0].NewSize);
		Assert.True(events[0].WidthChanged && events[0].HeightChanged);
		Assert.Equal(new Size(100, 50), events[1].PreviousSize);
		Assert.False(events[1].WidthChanged);
		Assert.True(events[1].HeightChanged);
		Assert.Equal(100, b.ActualWidth);
		Assert.Equal(80, b.ActualHeight);
	}

	[Fact]
	public void Same_size_arrange_at_new_offset_raises_nothing()
	{
		var b = new Border();
		Layout(b, 40, 30);
		int count = 0;
		b.SizeChanged += (_, _) => count++;

		Layout(b, 40, 30, x: 15);

		Assert.Equal(0, count);
	}

	[Fact]
	public void Aligned_arrange_raises_exactly_one_SizeChanged_with_final_size()
	{
		var b = new Border { Width = 40, HorizontalAlignment = HorizontalAlignment.Left };
		var events = new List<SizeChangedEventArgs>();
		b.SizeChanged += (_, e) => events.Add(e);

		Layout(b, 100, 30);

		var e0 = Assert.Single(events);
		Assert.Equal(new Size(40, 30), e0.NewSize);
		Assert.Equal(40, b.ActualWidth);
	}

	[Fact]
	public void SizeChangedEventArgs_routing_is_Direct_with_element_source()
	{
		Assert.Equal(RoutingStrategy.Direct, FrameworkElement.SizeChangedEvent.RoutingStrategy);
		Assert.Equal(typeof(SizeChangedEventHandler), FrameworkElement.SizeChangedEvent.HandlerType);
		var parent = new StackPanel();
		var child = new Border();
		parent.Children.Add(child);
		int parentCount = 0;
		parent.SizeChanged += (_, _) => parentCount++;
		SizeChangedEventArgs? args = null;
		child.SizeChanged += (_, e) => args = e;

		Layout(child, 10, 10);

		Assert.NotNull(args);
		Assert.Same(FrameworkElement.SizeChangedEvent, args!.RoutedEvent);
		Assert.Same(child, args.Source);
		Assert.Equal(0, parentCount);
	}

	[Fact]
	public void ActualWidthProperty_is_read_only_and_SetValue_throws()
	{
		var b = new Border();
		Assert.True(FrameworkElement.ActualWidthProperty.IsReadOnly);
		Assert.True(FrameworkElement.ActualHeightProperty.IsReadOnly);
		Assert.Throws<InvalidOperationException>(() => b.SetValue(FrameworkElement.ActualWidthProperty, 5.0));
		Assert.Throws<InvalidOperationException>(() => b.SetValue(FrameworkElement.ActualHeightProperty, 5.0));
	}

	[Fact]
	public void ActualSize_DPs_default_to_zero_owned_by_FrameworkElement()
	{
		Assert.Equal("ActualWidth", FrameworkElement.ActualWidthProperty.Name);
		Assert.Equal("ActualHeight", FrameworkElement.ActualHeightProperty.Name);
		Assert.Equal(typeof(FrameworkElement), FrameworkElement.ActualWidthProperty.OwnerType);
		Assert.Equal(typeof(double), FrameworkElement.ActualHeightProperty.PropertyType);
		Assert.Equal(0.0, FrameworkElement.ActualWidthProperty.DefaultMetadata.DefaultValue);
		var b = new Border();
		Assert.Equal(0.0, b.ActualWidth);
		Assert.Equal(0.0, b.GetValue(FrameworkElement.ActualHeightProperty));
	}

	[Fact]
	public void ElementName_binding_to_ActualWidth_updates_on_arrange()
	{
		var root = new StackPanel();
		var source = new Border();
		var target = new Sink();
		root.Children.Add(source);
		root.Children.Add(target);
		root.RegisterName("src", source);
		target.SetBinding(Sink.ValueProperty, new Binding("ActualWidth") { ElementName = "src" });
		Assert.Equal(0.0, target.Value);

		Layout(source, 120, 20);

		Assert.Equal(120.0, target.Value);
	}

	[Fact]
	public void SizeChangedInfo_round_trips_and_NewSize_reflects_RenderSize()
	{
		var b = new Border();
		Layout(b, 30, 20);

		var info = new SizeChangedInfo(b, new Size(1, 2), true, false);

		Assert.Same(b, info.Element);
		Assert.Equal(new Size(1, 2), info.PreviousSize);
		Assert.True(info.WidthChanged);
		Assert.False(info.HeightChanged);
		Assert.Equal(new Size(30, 20), info.NewSize);
	}

	[Fact]
	public void OnRenderSizeChanged_base_updates_ActualWidth_before_SizeChanged()
	{
		var b = new OrderBorder();
		b.SizeChanged += (_, _) => b.Log.Add("handler:" + b.GetValue(FrameworkElement.ActualWidthProperty));

		Layout(b, 64, 10);

		// RenderSize (and so ActualWidth) is already new; the base call publishes the DP before SizeChanged.
		Assert.Equal(new[] { "before-base:64/0", "handler:64", "after-base:64" }, b.Log);
	}

	[Fact]
	public void Setting_RenderSize_directly_only_stores_it()
	{
		// WPF: the RenderSize setter stores the size; SizeChanged comes only from layout.
		var b = new Border();
		SizeChangedEventArgs? args = null;
		b.SizeChanged += (_, e) => args = e;

		b.RenderSize = new Size(7, 9);

		Assert.Null(args);
		Assert.Equal(7, b.ActualWidth); // CLR getter reads RenderSize
		Assert.Equal(9, b.ActualHeight);
		Assert.Equal(0.0, b.GetValue(FrameworkElement.ActualWidthProperty)); // DP untouched
	}
}
