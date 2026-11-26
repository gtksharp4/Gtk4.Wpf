using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Round-15A compiler tests: <c>VisualStateManager.VisualStateGroups</c>
/// attached-property element form → group/state construction, naming and the
/// <c>SetVisualStateGroups</c> attach call. Each test fails without the feature.
/// </summary>
public class XamlCodeGeneratorVsmTests : XamlCodeGeneratorVsmTestBase
{
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
}
