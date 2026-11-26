using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Animation.AnimationTestHelpers;

namespace Gtk4.PresentationFramework.Tests.Media3D;

public partial class Media3DObjectModelTests
{
	[Fact]
	public void ModelVisual3D_owns_children_and_forwards_invalidation()
	{
		var parent = new ModelVisual3D();
		var child = new ModelVisual3D();
		parent.Children.Add(child);
		Assert.Throws<ArgumentException>(() => new ModelVisual3D().Children.Add(child));
		parent.Children.Remove(child);
		new ModelVisual3D().Children.Add(child);
		var content = new ModelVisual3D { Content = new GeometryModel3D() };
		Assert.IsType<GeometryModel3D>(content.Content);
		var attribute = (ContentPropertyAttribute)Attribute.GetCustomAttribute(
			typeof(ModelVisual3D), typeof(ContentPropertyAttribute))!;
		Assert.Equal("Children", attribute.Name);
		Assert.True(new ModelVisual3D().Transform.Value.IsIdentity);
		Assert.IsAssignableFrom<IAnimatable>(parent);
	}

	[Fact]
	public void Viewport3D_has_a_frozen_default_camera_and_clips_to_bounds_by_default()
	{
		var viewport = new Viewport3D();
		Assert.IsType<PerspectiveCamera>(viewport.Camera);
		Assert.True(viewport.Camera.IsFrozen);
		Assert.True(viewport.ClipToBounds);
		Assert.Empty(viewport.Children);
		viewport.Camera = new OrthographicCamera();
		Assert.IsType<OrthographicCamera>(viewport.Camera);
		var attribute = (ContentPropertyAttribute)Attribute.GetCustomAttribute(
			typeof(Viewport3D), typeof(ContentPropertyAttribute))!;
		Assert.Equal("Children", attribute.Name);
		Assert.Equal(typeof(Visual3DCollection), typeof(Viewport3D).GetProperty("Children")!.PropertyType);
		Assert.False(typeof(Viewport3D).GetProperty("Children")!.CanWrite);
	}

	[Fact]
	public void Clone_copies_values_and_nested_freezables()
	{
		var model = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(Brushes.Red));
		((MeshGeometry3D)model.Geometry!).Positions.Add(new Point3D(1, 2, 3));
		var clone = model.Clone();
		Assert.NotSame(model.Material, clone.Material);
		Assert.Single(((MeshGeometry3D)clone.Geometry!).Positions);
		model.Freeze();
		Assert.True(model.Geometry!.IsFrozen);
		Assert.True(model.Material!.IsFrozen);
		Assert.False(clone.IsFrozen);
	}
}
