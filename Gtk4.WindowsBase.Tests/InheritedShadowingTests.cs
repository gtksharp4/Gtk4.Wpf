using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Any layer above Inherited stops inheritance propagation to a child.</summary>
public class InheritedShadowingTests
{
	class Node : DependencyObject
	{
		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
			"Color", typeof(string), typeof(Node),
			new FrameworkPropertyMetadata("none", FrameworkPropertyMetadataOptions.Inherits, OnColorChanged));

		public int Callbacks;
		public int OnPropertyChangedCalls;
		public object? LastNew;

		public DependencyObject? Parent
		{
			get => InheritanceParent;
			set => InheritanceParent = value;
		}

		static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var n = (Node)d;
			n.Callbacks++;
			n.LastNew = e.NewValue;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == ColorProperty) OnPropertyChangedCalls++;
		}
	}

	[Fact]
	public void Style_valued_child_and_its_grandchild_are_not_notified()
	{
		var parent = new Node();
		var child = new Node { Parent = parent };
		var grand = new Node { Parent = child };
		child.SetStyleValue(Node.ColorProperty, "styled");
		child.Callbacks = grand.Callbacks = child.OnPropertyChangedCalls = 0;

		parent.SetValue(Node.ColorProperty, "red");

		Assert.Equal(0, child.Callbacks);
		Assert.Equal(0, child.OnPropertyChangedCalls);
		Assert.Equal(0, grand.Callbacks);
		Assert.Equal("styled", child.GetValue(Node.ColorProperty));
	}

	[Fact]
	public void Unshadowed_child_is_still_notified_with_the_new_value()
	{
		var parent = new Node();
		var child = new Node { Parent = parent };
		child.Callbacks = 0;

		parent.SetValue(Node.ColorProperty, "red");

		Assert.Equal(1, child.Callbacks);
		Assert.Equal("red", child.LastNew);
	}
}
