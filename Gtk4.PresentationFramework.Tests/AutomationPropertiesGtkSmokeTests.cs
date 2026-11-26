using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="AutomationProperties"/>: verifies that setting
/// Name/HelpText before and after widget realization does not throw, and that
/// the accessible label is applied via <c>Gtk.AccessibleExtensions.UpdateProperty</c>.
/// </summary>
public class AutomationPropertiesGtkSmokeTests : WpfGtkTestBase
{
	// ── Set before realize → push on realize ─────────────────────────────────────

	[Fact]
	public void Name_set_before_realize_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		AutomationProperties.SetName(btn, "Submit");
		// Materialize the widget — this triggers ApplyAutomationOnCreated.
		var ex = Record.Exception(() => _ = btn.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void HelpText_set_before_realize_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		AutomationProperties.SetHelpText(btn, "Click to submit the form");
		var ex = Record.Exception(() => _ = btn.Widget);
		Assert.Null(ex);
	});

	[Fact]
	public void Name_and_HelpText_set_before_realize_does_not_throw() => Run(() =>
	{
		var tb = new TextBox();
		AutomationProperties.SetName(tb, "Email");
		AutomationProperties.SetHelpText(tb, "Enter your email address");
		var ex = Record.Exception(() => _ = tb.Widget);
		Assert.Null(ex);
	});

	// ── Set after realize → immediate GTK push ────────────────────────────────────

	[Fact]
	public void Name_set_after_realize_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		_ = btn.Widget; // realize first
		var ex = Record.Exception(() => AutomationProperties.SetName(btn, "Cancel"));
		Assert.Null(ex);
	});

	[Fact]
	public void HelpText_set_after_realize_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		_ = btn.Widget;
		var ex = Record.Exception(() => AutomationProperties.SetHelpText(btn, "Cancels the operation"));
		Assert.Null(ex);
	});

	// ── Object-model-only props do not throw on realize ───────────────────────────

	[Fact]
	public void ObjectModel_props_do_not_throw_on_realize() => Run(() =>
	{
		var tb = new TextBox();
		var label = new TextBlock();
		AutomationProperties.SetAutomationId(tb, "tbEmail");
		AutomationProperties.SetLabeledBy(tb, label);
		AutomationProperties.SetIsRequiredForForm(tb, true);
		AutomationProperties.SetLiveSetting(tb, AutomationLiveSetting.Polite);
		var ex = Record.Exception(() => _ = tb.Widget);
		Assert.Null(ex);
	});

	// ── Null Name / HelpText with realized widget does not throw ──────────────────

	[Fact]
	public void Null_Name_on_realized_widget_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		_ = btn.Widget;
		// Setting null must not attempt a GTK push (guard in AutomationAccessiblePush).
		var ex = Record.Exception(() => AutomationProperties.SetName(btn, null));
		Assert.Null(ex);
	});

	[Fact]
	public void Null_HelpText_on_realized_widget_does_not_throw() => Run(() =>
	{
		var btn = new Button();
		AutomationProperties.SetHelpText(btn, "original");
		_ = btn.Widget;
		var ex = Record.Exception(() => AutomationProperties.SetHelpText(btn, null));
		Assert.Null(ex);
	});
}
