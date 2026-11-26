using System.Windows.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public class AutomationIdentifierTests
{
	[Fact]
	public void ControlType_ids_names_and_localized_names_match_UIA()
	{
		Assert.Equal(50000, ControlType.Button.Id);
		Assert.Equal("ControlType.Button", ControlType.Button.ProgrammaticName);
		Assert.Equal("check box", ControlType.CheckBox.LocalizedControlType);
		Assert.Equal(50038, ControlType.Separator.Id);
		Assert.Same(ControlType.Window, ControlType.LookupById(50032));
		Assert.Null(ControlType.LookupById(1));
	}

	[Fact]
	public void Properties_are_found_by_id()
	{
		Assert.Equal(30005, AutomationElementIdentifiers.NameProperty.Id);
		Assert.Equal("AutomationElementIdentifiers.NameProperty",
			AutomationElementIdentifiers.NameProperty.ProgrammaticName);
		Assert.Same(AutomationElementIdentifiers.IsEnabledProperty, AutomationProperty.LookupById(30010));
		Assert.Same(TogglePatternIdentifiers.ToggleStateProperty, AutomationProperty.LookupById(30086));
		Assert.Same(RangeValuePatternIdentifiers.ValueProperty, AutomationProperty.LookupById(30047));
	}

	[Fact]
	public void Patterns_and_events_are_found_by_id()
	{
		Assert.Same(InvokePatternIdentifiers.Pattern, AutomationPattern.LookupById(10000));
		Assert.Same(WindowPatternIdentifiers.Pattern, AutomationPattern.LookupById(10009));
		Assert.Equal("TogglePatternIdentifiers.Pattern", TogglePatternIdentifiers.Pattern.ProgrammaticName);
		Assert.Same(InvokePatternIdentifiers.InvokedEvent, AutomationEvent.LookupById(20009));
		Assert.Same(AutomationElementIdentifiers.StructureChangedEvent, AutomationEvent.LookupById(20002));
		Assert.Null(AutomationPattern.LookupById(7));
	}

	[Fact]
	public void Enums_keep_the_UIA_values()
	{
		Assert.Equal(2, (int)ToggleState.Indeterminate);
		Assert.Equal(3, (int)ExpandCollapseState.LeafNode);
		Assert.Equal(4, (int)ScrollAmount.SmallIncrement);
		Assert.Equal(2, (int)WindowVisualState.Minimized);
		Assert.Equal(-1, ScrollPatternIdentifiers.NoScroll);
	}
}
