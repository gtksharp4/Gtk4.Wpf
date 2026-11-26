using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-15A compiler tests: storyboard children of a <c>VisualState</c>,
/// multiple groups, and named-field declarations for <c>x:Name</c>-bearing
/// groups/states. Each test fails without the feature.
/// </summary>
public class XamlCodeGeneratorVsmStoryboardTests : XamlCodeGeneratorVsmTestBase
{
	// ── Storyboard child ──────────────────────────────────────────────────────

	[Fact]
	public void Vsm_state_with_Storyboard_emits_Storyboard_construction()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""MouseOver"">
      <Storyboard>
        <DoubleAnimation Storyboard.TargetName=""myEl""
                         Storyboard.TargetProperty=""Opacity""
                         From=""1"" To=""0.5"" Duration=""0:0:0.2"" />
      </Storyboard>
    </VisualState>
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Media.Animation.Storyboard()", code);
	}

	[Fact]
	public void Vsm_state_Storyboard_assigned_to_state()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""MouseOver"">
      <Storyboard>
        <DoubleAnimation Storyboard.TargetProperty=""Opacity""
                         From=""1"" To=""0.5"" Duration=""0:0:0.2"" />
      </Storyboard>
    </VisualState>
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Storyboard = ", code);
	}

	[Fact]
	public void Vsm_state_without_Storyboard_emits_no_Storyboard()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		// Scoped to the VSM codegen: no storyboard construction and no per-state
		// Storyboard assignment (asserting the whole file lacks "Storyboard" is
		// brittle — unrelated boilerplate could contain the substring).
		Assert.DoesNotContain("new System.Windows.Media.Animation.Storyboard()", code);
		Assert.DoesNotContain(".Storyboard = ", code);
	}

	// ── Multiple groups ───────────────────────────────────────────────────────

	[Fact]
	public void Vsm_multiple_groups_emit_multiple_VisualStateGroup_constructions()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
  <VisualStateGroup x:Name=""FocusStates"">
    <VisualState x:Name=""Unfocused"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		int count = 0;
		int idx = 0;
		while ((idx = code.IndexOf("new System.Windows.VisualStateGroup()", idx)) >= 0)
		{ count++; idx++; }
		Assert.Equal(2, count);
	}

	// ── Named fields declared ─────────────────────────────────────────────────

	[Fact]
	public void Vsm_named_group_produces_field_declaration()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"" />
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		// CollectNamedElements picks up VisualStateGroup with x:Name.
		Assert.Contains("System.Windows.VisualStateGroup CommonStates", code);
	}

	[Fact]
	public void Vsm_named_state_produces_field_declaration()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("System.Windows.VisualState Normal", code);
	}
}
