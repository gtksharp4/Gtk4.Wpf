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
	public void Transform_defaults_match_wpf()
	{
		Assert.True(Transform3D.Identity.IsFrozen);
		Assert.True(Transform3D.Identity.Value.IsIdentity);
		var scale = new ScaleTransform3D();
		Assert.Equal((1.0, 1.0, 1.0), (scale.ScaleX, scale.ScaleY, scale.ScaleZ));
		Assert.Equal((0.0, 0.0, 0.0), (scale.CenterX, scale.CenterY, scale.CenterZ));
		Assert.Equal((0.0, 0.0, 0.0), (new TranslateTransform3D().OffsetX, new TranslateTransform3D().OffsetY, 0.0));
		var rotate = new RotateTransform3D();
		Assert.Same(Rotation3D.Identity, rotate.Rotation);
		Assert.True(Rotation3D.Identity.IsFrozen);
		var axis = new AxisAngleRotation3D();
		Assert.Equal(new Vector3D(0, 1, 0), axis.Axis);
		Assert.Equal(0.0, axis.Angle);
		Assert.True(new QuaternionRotation3D().Quaternion.IsIdentity);
		Assert.True(new MatrixTransform3D().Matrix.IsIdentity);
	}

	[Fact]
	public void Transform3DGroup_composes_children_in_order_and_has_a_content_property()
	{
		var group = new Transform3DGroup();
		group.Children.Add(new ScaleTransform3D(2, 2, 2));
		group.Children.Add(new TranslateTransform3D(1, 0, 0));
		Assert.Equal(new Point3D(3, 2, 2), group.Transform(new Point3D(1, 1, 1)));
		Assert.True(group.IsAffine);
		var attribute = (ContentPropertyAttribute)Attribute.GetCustomAttribute(
			typeof(Transform3DGroup), typeof(ContentPropertyAttribute))!;
		Assert.Equal("Children", attribute.Name);
		Assert.NotNull(new TranslateTransform3D(1, 2, 3).Inverse);
		Assert.Null(new ScaleTransform3D(0, 1, 1).Inverse);
	}

	[Fact]
	public void RotateTransform3D_rotates_about_its_centre()
	{
		var rotate = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180), 1, 0, 0);
		var p = rotate.Transform(new Point3D(0, 0, 0));
		Assert.Equal(2.0, p.X, 9);
		Assert.Equal(0.0, p.Y, 9);
	}

	[Fact]
	public void Animating_an_axis_angle_updates_Value_and_raises_Changed_up_the_owner_chain()
	{
		var angle = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
		var rotate = new RotateTransform3D(angle);
		var model = new GeometryModel3D { Transform = rotate };
		int changes = 0;
		model.Changed += (s, e) => changes++;
		var clock = new DoubleAnimation(0, 90, TimeSpan.FromSeconds(1)).CreateClock();
		angle.ApplyAnimationClock(AxisAngleRotation3D.AngleProperty, clock);
		Tick(clock, 0.5);
		Assert.Equal(45.0, angle.Angle, 9);
		Assert.True(changes > 0);
		var p = rotate.Value.Transform(new Point3D(1, 0, 0));
		Assert.Equal(Math.Cos(Math.PI / 4), p.X, 9);
	}

	[Fact]
	public void Freezing_makes_the_value_collections_read_only()
	{
		var points = new Point3DCollection { new Point3D(1, 2, 3) };
		int changed = 0;
		points.Changed += (s, e) => changed++;
		points.Add(new Point3D());
		Assert.Equal(1, changed);
		points.Freeze();
		Assert.Throws<InvalidOperationException>(() => points.Add(new Point3D()));
		Assert.True(((System.Collections.IList)points).IsReadOnly);
		var copy = points.Clone();
		Assert.False(copy.IsFrozen);
		Assert.Equal(2, copy.Count);
		var ints = new Int32Collection(new[] { 1, 2, 3 });
		Assert.Equal(1, ints.IndexOf(2));
		Assert.Throws<ArgumentOutOfRangeException>(() => new Vector3DCollection(-1));
	}

	[Fact]
	public void Freezable_collections_forward_child_changes()
	{
		var group = new Model3DGroup();
		var child = new GeometryModel3D();
		group.Children.Add(child);
		int changes = 0;
		group.Changed += (s, e) => changes++;
		child.Material = new DiffuseMaterial();
		Assert.True(changes > 0);
		int before = changes;
		group.Children.Remove(child);
		int afterRemove = changes;
		child.Material = new EmissiveMaterial();
		Assert.Equal(afterRemove, changes);
		Assert.True(afterRemove > before);
	}

	[Fact]
	public void Material_defaults_and_nested_brush_changes()
	{
		var diffuse = new DiffuseMaterial(Brushes.Red);
		Assert.Equal(Colors.White, diffuse.Color);
		Assert.Equal(Colors.White, diffuse.AmbientColor);
		Assert.Equal(Colors.White, new EmissiveMaterial().Color);
		Assert.Equal(40.0, new SpecularMaterial().SpecularPower);
		var brush = new SolidColorBrush(Colors.Blue);
		var material = new DiffuseMaterial(brush);
		int changes = 0;
		material.Changed += (s, e) => changes++;
		brush.Color = Colors.Green;
		Assert.True(changes > 0);
		var group = new MaterialGroup();
		group.Children.Add(material);
		Assert.Single(group.Children);
	}

	[Fact]
	public void Light_defaults_match_wpf()
	{
		Assert.Equal(Colors.White, new AmbientLight().Color);
		Assert.Equal(new Vector3D(0, 0, -1), new DirectionalLight().Direction);
		var point = new PointLight();
		Assert.Equal(new Point3D(0, 0, 0), point.Position);
		Assert.Equal(double.PositiveInfinity, point.Range);
		Assert.Equal((1.0, 0.0, 0.0), (point.ConstantAttenuation, point.LinearAttenuation, point.QuadraticAttenuation));
		var spot = new SpotLight();
		Assert.Equal((90.0, 180.0), (spot.OuterConeAngle, spot.InnerConeAngle));
		Assert.Equal(new Vector3D(0, 0, -1), spot.Direction);
		var directional = new DirectionalLight(Colors.Red, new Vector3D(1, 0, 0));
		Assert.Equal(Colors.Red, directional.Color);
	}

	[Fact]
	public void Camera_defaults_and_constructors_match_wpf()
	{
		var camera = new PerspectiveCamera();
		Assert.Equal(new Point3D(0, 0, 0), camera.Position);
		Assert.Equal(new Vector3D(0, 0, -1), camera.LookDirection);
		Assert.Equal(new Vector3D(0, 1, 0), camera.UpDirection);
		Assert.Equal(0.125, camera.NearPlaneDistance);
		Assert.Equal(double.PositiveInfinity, camera.FarPlaneDistance);
		Assert.Equal(45.0, camera.FieldOfView);
		Assert.Equal(2.0, new OrthographicCamera().Width);
		Assert.True(new MatrixCamera().ViewMatrix.IsIdentity);
		var full = new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 60);
		Assert.Equal(60.0, full.FieldOfView);
		Assert.Equal(5.0, full.Position.Z);
	}

	[Fact]
	public void Mesh_bounds_is_the_box_of_its_positions_and_empty_when_there_are_none()
	{
		var mesh = new MeshGeometry3D();
		Assert.True(mesh.Bounds.IsEmpty);
		mesh.Positions.Add(new Point3D(0, 0, 0));
		mesh.Positions.Add(new Point3D(2, 4, 6));
		Assert.Equal(new Rect3D(0, 0, 0, 2, 4, 6), mesh.Bounds);
		var model = new GeometryModel3D(mesh, new DiffuseMaterial()) { Transform = new TranslateTransform3D(1, 0, 0) };
		Assert.Equal(new Rect3D(1, 0, 0, 2, 4, 6), model.Bounds);
		Assert.Empty(new MeshGeometry3D().TextureCoordinates);
	}

	[Fact]
	public void Shared_default_collections_are_not_mutated_by_instances()
	{
		var a = new MeshGeometry3D();
		var b = new MeshGeometry3D();
		a.Positions.Add(new Point3D(1, 1, 1));
		Assert.Empty(b.Positions);
		var frozen = new MeshGeometry3D();
		frozen.Freeze();
		Assert.Empty(frozen.Positions);
		Assert.Throws<InvalidOperationException>(() => frozen.Positions.Add(new Point3D()));
	}
}
