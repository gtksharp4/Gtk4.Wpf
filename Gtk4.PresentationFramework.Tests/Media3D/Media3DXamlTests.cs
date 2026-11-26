using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>The runtime XAML loader builds Media3D object graphs (the compiled path is the generator's).</summary>
public class Media3DXamlTests
{
	const string Scene = @"
<Viewport3D xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
  <Viewport3D.Camera>
    <PerspectiveCamera Position='0,0,5' LookDirection='0,0,-1' UpDirection='0,1,0' FieldOfView='60' />
  </Viewport3D.Camera>
  <ModelVisual3D>
    <ModelVisual3D.Content>
      <Model3DGroup>
        <AmbientLight Color='White' />
        <GeometryModel3D>
          <GeometryModel3D.Geometry>
            <MeshGeometry3D Positions='-1,-1,0 1,-1,0 0,1,0' TriangleIndices='0 1 2' />
          </GeometryModel3D.Geometry>
          <GeometryModel3D.Material>
            <DiffuseMaterial Brush='Red' />
          </GeometryModel3D.Material>
        </GeometryModel3D>
      </Model3DGroup>
    </ModelVisual3D.Content>
    <ModelVisual3D.Transform>
      <RotateTransform3D>
        <RotateTransform3D.Rotation>
          <AxisAngleRotation3D Axis='0,1,0' Angle='30' />
        </RotateTransform3D.Rotation>
      </RotateTransform3D>
    </ModelVisual3D.Transform>
  </ModelVisual3D>
</Viewport3D>";

	[Fact]
	public void A_viewport_scene_loads_with_property_elements_and_converted_attributes()
	{
		var viewport = (Viewport3D)XamlReader.Parse(Scene);
		var camera = Assert.IsType<PerspectiveCamera>(viewport.Camera);
		Assert.Equal(new Point3D(0, 0, 5), camera.Position);
		Assert.Equal(60.0, camera.FieldOfView);

		var visual = Assert.IsType<ModelVisual3D>(Assert.Single(viewport.Children));
		var rotate = Assert.IsType<RotateTransform3D>(visual.Transform);
		Assert.Equal(30.0, Assert.IsType<AxisAngleRotation3D>(rotate.Rotation).Angle);

		var group = Assert.IsType<Model3DGroup>(visual.Content);
		Assert.Equal(2, group.Children.Count);
		var model = Assert.IsType<GeometryModel3D>(group.Children[1]);
		var mesh = Assert.IsType<MeshGeometry3D>(model.Geometry);
		Assert.Equal(3, mesh.Positions.Count);
		Assert.Equal(new[] { 0, 1, 2 }, mesh.TriangleIndices);
		Assert.IsType<DiffuseMaterial>(model.Material);
	}

	[Fact]
	public void Key_frame_animations_and_key_times_load_from_xaml()
	{
		const string xaml = @"
<DoubleAnimationUsingKeyFrames xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                               Duration='0:0:4'>
  <LinearDoubleKeyFrame Value='10' KeyTime='50%' />
  <SplineDoubleKeyFrame Value='20' KeyTime='0:0:4' KeySpline='0.1,0.2 0.3,0.4' />
</DoubleAnimationUsingKeyFrames>";
		var animation = (DoubleAnimationUsingKeyFrames)XamlReader.Parse(xaml);
		Assert.Equal(2, animation.KeyFrames.Count);
		Assert.Equal(KeyTime.FromPercent(0.5), animation.KeyFrames[0].KeyTime);
		var spline = Assert.IsType<SplineDoubleKeyFrame>(animation.KeyFrames[1]);
		Assert.Equal(new System.Windows.Point(0.3, 0.4), spline.KeySpline.ControlPoint2);
		Assert.Equal(TimeSpan.FromSeconds(4), spline.KeyTime.TimeSpan);
	}
}
