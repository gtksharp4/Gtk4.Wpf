using Gtk4.Forms.MarkupCompiler;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>
/// Round-15A compiler tests: <c>VisualStateManager.VisualStateGroups</c>
/// attached-property element form → <c>InitializeComponent</c> codegen.
/// Each test fails without the matching feature.
/// </summary>
public class XamlCodeGeneratorVsmTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Grid(string body) =>
		$@"<Grid x:Class=""MyApp.MyView""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Grid>";

	// ── Group and state construction ──────────────────────────────────────────

	[Fact]
	public void Vsm_emits_VisualStateGroup_construction()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"" />
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.VisualStateGroup()", code);
	}

	[Fact]
	public void Vsm_emits_VisualState_construction()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.VisualState()", code);
	}

	[Fact]
	public void Vsm_emits_group_Name_assignment()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"" />
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@".Name = ""CommonStates""", code);
	}

	[Fact]
	public void Vsm_emits_state_Name_assignment()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(@".Name = ""Normal""", code);
	}

	// ── States.Add / list population ─────────────────────────────────────────

	[Fact]
	public void Vsm_emits_States_Add()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
    <VisualState x:Name=""Pressed"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".States.Add(", code);
	}

	[Fact]
	public void Vsm_emits_list_Add_for_group()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		// The group is added to the list variable.
		Assert.Contains(".Add(", code);
	}

	// ── SetVisualStateGroups call ─────────────────────────────────────────────

	[Fact]
	public void Vsm_emits_SetVisualStateGroups_call()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("System.Windows.VisualStateManager.SetVisualStateGroups(", code);
	}

	[Fact]
	public void Vsm_SetVisualStateGroups_targets_this()
	{
		string xaml = Grid(@"
<VisualStateManager.VisualStateGroups>
  <VisualStateGroup x:Name=""CommonStates"">
    <VisualState x:Name=""Normal"" />
  </VisualStateGroup>
</VisualStateManager.VisualStateGroups>");

		string code = _gen.Generate(xaml, out _);

		// The parent variable of a root-element VSM block is "this".
		Assert.Contains("SetVisualStateGroups(this,", code);
	}

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

		Assert.DoesNotContain("Storyboard", code);
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
