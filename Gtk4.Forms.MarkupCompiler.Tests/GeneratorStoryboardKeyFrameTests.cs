using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Key-frame animations, bound animation values and named freezables in storyboards (KeySplineAnimations).</summary>
public class GeneratorStoryboardKeyFrameTests : GeneratorV2TestBase
{
	const string Stub = "namespace App { public partial class P { } }";

	const string Xaml = "<Page " + Head + @" x:Class=""App.P"">
<Page.Resources>
	<Storyboard x:Key=""sb"">
		<DoubleAnimationUsingKeyFrames Storyboard.TargetName=""T"" Storyboard.TargetProperty=""Y"" Duration=""0:0:4"">
			<DiscreteDoubleKeyFrame Value=""100"" KeyTime=""0%"" />
			<SplineDoubleKeyFrame x:Name=""spl"" Value=""10"" KeyTime=""100%"" KeySpline=""0.1,0.2 0.3,0.4"" />
		</DoubleAnimationUsingKeyFrames>
		<DoubleAnimation Storyboard.TargetName=""b"" Storyboard.TargetProperty=""Width""
			To=""{Binding ElementName=b, Path=ActualWidth}"" Duration=""0:0:1"" />
	</Storyboard>
</Page.Resources>
<StackPanel>
	<Button Name=""b"" />
	<Path><Path.Data><GeometryGroup><LineGeometry><LineGeometry.Transform>
		<TranslateTransform x:Name=""T"" Y=""1"" /></LineGeometry.Transform></LineGeometry></GeometryGroup></Path.Data></Path>
</StackPanel>
</Page>";

	[Fact]
	public void Key_frame_animation_is_emitted_with_its_frames_and_names()
	{
		string code = Gen(Xaml, sources: new[] { Stub });

		Assert.Contains("new System.Windows.Media.Animation.DoubleAnimationUsingKeyFrames()", code);
		Assert.Contains("DiscreteDoubleKeyFrame()", code);
		Assert.Contains("SplineDoubleKeyFrame()", code);
		Assert.Contains(".KeyFrames.Add(", code);
		Assert.Contains("this.spl = ", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void Binding_on_an_animation_value_binds_the_dependency_property()
	{
		string code = Gen(Xaml, sources: new[] { Stub });

		Assert.Contains("NonElementBinding.TrySetBinding(__anim", code);
		Assert.Contains("DoubleAnimation.ToProperty", code);
		Assert.DoesNotContain(".To = \"{Binding", code);
	}

	[Fact]
	public void Name_on_a_freezable_in_a_geometry_property_element_is_registered()
	{
		string code = Gen(Xaml, sources: new[] { Stub });

		Assert.Contains("RegisterName(\"T\"", code);
		Assert.Contains("this.T = ", code);
	}

	[Fact]
	public void A_binding_on_a_non_element_is_reported_as_not_evaluated()
	{
		var msgs = Messages("<Page " + Head + @" x:Class=""App.P""><StackPanel><TextBox x:Name=""t""/>
<Border><Border.RenderTransform><RotateTransform Angle=""{Binding Text, ElementName=t}""/></Border.RenderTransform>
</Border></StackPanel></Page>");

		Assert.Contains(msgs, m => m.Contains("binding on non-element") && m.Contains("RotateTransform.Angle")
			&& m.Contains("is not evaluated"));
	}
}
