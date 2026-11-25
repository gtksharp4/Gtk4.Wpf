using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>A storyboard of ParallelTimelines that target named 3D freezables (HexSphereDemo).</summary>
public class GeneratorParallelTimelineTests : GeneratorV2TestBase
{
	const string Stub = "namespace App { public partial class P { } }";

	const string Xaml = "<Page " + Head + @" x:Class=""App.P"">
<Page.Resources>
	<Storyboard x:Key=""RotateStoryboard"">
		<ParallelTimeline RepeatBehavior=""Forever"" Storyboard.TargetName=""myRotateInner"" Storyboard.TargetProperty=""Angle"">
			<DoubleAnimation From=""0"" To=""360"" Duration=""0:0:20"" />
		</ParallelTimeline>
		<ParallelTimeline RepeatBehavior=""Forever"" Storyboard.TargetName=""myRotateOuter"" Storyboard.TargetProperty=""Angle"">
			<DoubleAnimation From=""0"" To=""360"" Duration=""0:0:30"" />
		</ParallelTimeline>
	</Storyboard>
</Page.Resources>
<Viewport3D>
	<ModelVisual3D>
		<ModelVisual3D.Content><Model3DGroup><Model3DGroup.Children>
			<GeometryModel3D>
				<GeometryModel3D.Transform>
					<Transform3DGroup>
						<Transform3DGroup.Children>
							<ScaleTransform3D ScaleX=""10"" ScaleY=""10"" ScaleZ=""10"" />
							<RotateTransform3D>
								<RotateTransform3D.Rotation>
									<AxisAngleRotation3D x:Name=""myRotateInner"" Axis=""1 0 0"" Angle=""0"" />
								</RotateTransform3D.Rotation>
							</RotateTransform3D>
						</Transform3DGroup.Children>
					</Transform3DGroup>
				</GeometryModel3D.Transform>
			</GeometryModel3D>
			<Model3DGroup x:Name=""g"">
				<Model3DGroup.Transform>
					<Transform3DGroup>
						<Transform3DGroup.Children>
							<Transform3DCollection>
								<RotateTransform3D>
									<RotateTransform3D.Rotation>
										<AxisAngleRotation3D x:Name=""myRotateOuter"" Axis=""0 1 0"" Angle=""0"" />
									</RotateTransform3D.Rotation>
								</RotateTransform3D>
							</Transform3DCollection>
						</Transform3DGroup.Children>
					</Transform3DGroup>
				</Model3DGroup.Transform>
			</Model3DGroup>
		</Model3DGroup.Children></Model3DGroup></ModelVisual3D.Content>
	</ModelVisual3D>
</Viewport3D>
</Page>";

	[Fact]
	public void Parallel_timelines_are_emitted_with_their_children_and_targets()
	{
		string code = Gen(Xaml, sources: new[] { Stub });

		Assert.Equal(2, Count(code, "new System.Windows.Media.Animation.ParallelTimeline()"));
		Assert.Equal(2, Count(code, "new System.Windows.Media.Animation.DoubleAnimation()"));
		Assert.Contains("SetTargetName(__anim", code);
		Assert.Contains("\"myRotateInner\"", code);
		Assert.Contains("\"myRotateOuter\"", code);
		Assert.Contains("RepeatBehavior = ", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void Named_rotations_nested_in_transform_groups_are_registered_for_both_collection_forms()
	{
		string code = Gen(Xaml, sources: new[] { Stub });

		Assert.Contains("RegisterName(\"myRotateInner\"", code);
		Assert.Contains("RegisterName(\"myRotateOuter\"", code);
		Assert.Contains("this.myRotateInner = ", code);
		Assert.Contains("this.myRotateOuter = ", code);
	}

	static int Count(string text, string part)
	{
		int n = 0, i = 0;
		while ((i = text.IndexOf(part, i, System.StringComparison.Ordinal)) >= 0) { n++; i += part.Length; }
		return n;
	}
}
