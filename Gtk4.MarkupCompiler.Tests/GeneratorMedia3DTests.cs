using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Media3D object graphs are built by generated code (Graphics/Geometry).</summary>
public class GeneratorMedia3DTests : GeneratorV2TestBase
{
	internal const string Scene = "<Page " + Head + @" x:Class=""App.Scene"">
<Page.Resources>
	<MeshGeometry3D x:Key=""Tri"" Positions=""0,0,0 1,0,0 0,1,0"" TriangleIndices=""0 1 2"" />
</Page.Resources>
<Viewport3D Name=""vp"">
	<Viewport3D.Camera>
		<PerspectiveCamera LookDirection=""0,0,-2"" UpDirection=""0,1,0"" Position=""0,0,5"" FieldOfView=""90"" />
	</Viewport3D.Camera>
	<ModelVisual3D>
		<ModelVisual3D.Children>
			<ModelVisual3D>
				<ModelVisual3D.Content><AmbientLight Color=""#FFFFFFFF"" /></ModelVisual3D.Content>
			</ModelVisual3D>
			<ModelVisual3D>
				<ModelVisual3D.Content>
					<GeometryModel3D x:Name=""myPlane"">
						<GeometryModel3D.Geometry>
							<MeshGeometry3D x:Name=""myGeometry"" TriangleIndices=""0,1,2 3,4,5""
								TextureCoordinates=""0,0 0,-1 -1,-1 -1,-1 -1,0 0,0""
								Positions=""12,-9,0 12,9,0 -12,9,0 -12,9,0 -12,-9,0 12,-9,0"" />
						</GeometryModel3D.Geometry>
						<GeometryModel3D.Material>
							<DiffuseMaterial Brush=""Red"" />
						</GeometryModel3D.Material>
					</GeometryModel3D>
				</ModelVisual3D.Content>
				<ModelVisual3D.Transform>
					<Transform3DGroup>
						<RotateTransform3D x:Name=""rot"" CenterX=""0"">
							<RotateTransform3D.Rotation>
								<AxisAngleRotation3D Axis=""0,1,0"" Angle=""0"" x:Name=""axis"" />
							</RotateTransform3D.Rotation>
						</RotateTransform3D>
						<ScaleTransform3D ScaleX=""1"" ScaleY=""1"" ScaleZ=""1"" />
					</Transform3DGroup>
				</ModelVisual3D.Transform>
			</ModelVisual3D>
		</ModelVisual3D.Children>
	</ModelVisual3D>
</Viewport3D>
</Page>";

	[Fact]
	public void Viewport_scene_emits_camera_content_transform_and_assigns_nested_names()
	{
		string code = Gen(Scene, sources: new[] { "namespace App { public partial class Scene { } }" });

		Assert.Contains("PerspectiveCamera", code);
		Assert.Contains("Viewport3D.CameraProperty, ", code);
		Assert.DoesNotContain("Resources[\"Tri\"] = \"\"", code);
		Assert.Contains(".Content = ", code);
		Assert.Contains(".Transform = ", code);
		Assert.Contains("myGeometry = ", code);
		Assert.Contains("RegisterName(\"myGeometry\"", code);
		AssertCompiles(code, "namespace App { public partial class Scene { } }");
	}
}
