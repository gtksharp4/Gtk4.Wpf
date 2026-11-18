using System.Windows.Automation;

namespace System.Windows;

/// <summary>
/// <see cref="Automation.AutomationProperties"/> GTK application hook for <see cref="UIElement"/>.
/// <para>
/// When the backing widget is first created, any <c>Name</c> or <c>HelpText</c> values set
/// before realization are pushed to the GTK 4 accessible API via
/// <see cref="AutomationAccessiblePush.Push"/>. See <see cref="Automation.AutomationProperties"/>
/// for the full list of which properties push to GTK vs. object-model-only.
/// </para>
/// </summary>
public abstract partial class UIElement
{
	/// <summary>
	/// Called from <see cref="OnWidgetCreated"/> to apply any
	/// <see cref="Automation.AutomationProperties"/> values set before widget creation.
	/// Implemented in this file; wired via partial method in <c>UIElement.cs</c>.
	/// </summary>
	partial void ApplyAutomationOnCreated(Gtk.Widget widget) =>
		AutomationAccessiblePush.Push(widget, this);
}
