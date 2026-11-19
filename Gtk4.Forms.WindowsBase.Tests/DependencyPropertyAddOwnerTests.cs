using System;
using System.Windows;
using Xunit;

namespace Gtk4.Forms.WindowsBase.Tests;

public class DependencyPropertyAddOwnerTests
{
	class OwnerBase : DependencyObject
	{
		public static readonly DependencyProperty SizeProp = DependencyProperty.RegisterAttached(
			"AoSize", typeof(double), typeof(OwnerBase), new FrameworkPropertyMetadata(12.0, true));
	}

	class OwnerA : DependencyObject
	{
		public static readonly DependencyProperty SizeProp = OwnerBase.SizeProp.AddOwner(typeof(OwnerA));
	}

	class OwnerB : DependencyObject
	{
		public static readonly DependencyProperty SizeProp =
			OwnerBase.SizeProp.AddOwner(typeof(OwnerB), new FrameworkPropertyMetadata(20.0, true));
	}

	class OwnerC : DependencyObject { }

	class OwnerSub : OwnerB { }

	[Fact]
	public void AddOwner_returns_same_instance()
	{
		Assert.Same(OwnerBase.SizeProp, OwnerA.SizeProp);
		Assert.Same(OwnerBase.SizeProp, OwnerB.SizeProp);
	}

	[Fact]
	public void AddOwner_registers_name_for_new_owner()
	{
		_ = OwnerA.SizeProp;
		Assert.Same(OwnerBase.SizeProp, DependencyProperty.FindByName("AoSize", typeof(OwnerA)));
	}

	[Fact]
	public void AddOwner_metadata_overrides_default_only_for_that_owner_subtree()
	{
		_ = OwnerB.SizeProp;
		Assert.Equal(20.0, new OwnerB().GetValue(OwnerBase.SizeProp));
		Assert.Equal(20.0, new OwnerSub().GetValue(OwnerBase.SizeProp));
		Assert.Equal(12.0, new OwnerC().GetValue(OwnerBase.SizeProp));
	}

	[Fact]
	public void AddOwner_leaves_original_owner_metadata_unchanged()
	{
		_ = OwnerB.SizeProp;
		Assert.Equal(12.0, OwnerBase.SizeProp.DefaultMetadata.DefaultValue);
		Assert.Equal(12.0, new OwnerBase().GetValue(OwnerBase.SizeProp));
		Assert.Equal(typeof(OwnerBase), OwnerBase.SizeProp.OwnerType);
	}

	[Fact]
	public void AddOwner_null_owner_throws()
	{
		Assert.Throws<ArgumentNullException>(() => OwnerBase.SizeProp.AddOwner(null!));
		Assert.Throws<ArgumentNullException>(() =>
			OwnerBase.SizeProp.AddOwner(null!, new PropertyMetadata(1.0)));
	}

	[Fact]
	public void AddOwner_duplicate_owner_throws_ArgumentException()
	{
		_ = OwnerA.SizeProp;
		Assert.Throws<ArgumentException>(() => OwnerBase.SizeProp.AddOwner(typeof(OwnerA)));
		Assert.Throws<ArgumentException>(() => OwnerBase.SizeProp.AddOwner(typeof(OwnerBase)));
	}

	[Fact]
	public void AddOwner_inheriting_metadata_flows_through_inheritance_parent()
	{
		var parent = new OwnerB();
		var child = new OwnerSub();
		child.InheritanceParent = parent;
		parent.SetValue(OwnerBase.SizeProp, 33.0);
		Assert.Equal(33.0, child.GetValue(OwnerBase.SizeProp));
	}
}
