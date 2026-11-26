using System;
using System.Linq;
using System.Windows.Automation;

namespace Gtk4.Wpf.Testing;

/// <summary>Locator sugar over <c>FindFirst</c>/<c>FindAll</c>. Scoping is the element the call is made on.</summary>
public static class ElementFinder
{
	/// <summary>Returns the first descendant matching <paramref name="by"/>, or throws with the nearby tree.</summary>
	public static AutomationElement Find(this AutomationElement scope, By by) =>
		scope.TryFind(by) ?? throw NotFound(scope, by);

	/// <summary>Returns the first descendant matching <paramref name="by"/>, or <c>null</c>.</summary>
	public static AutomationElement? TryFind(this AutomationElement scope, By by)
	{
		if (scope == null) throw new ArgumentNullException(nameof(scope));
		if (by == null) throw new ArgumentNullException(nameof(by));
		return scope.FindFirst(TreeScope.Descendants, by.Condition);
	}

	/// <summary>Returns every descendant matching <paramref name="by"/> (possibly none).</summary>
	public static AutomationElement[] FindAll(this AutomationElement scope, By by)
	{
		if (scope == null) throw new ArgumentNullException(nameof(scope));
		if (by == null) throw new ArgumentNullException(nameof(by));
		return scope.FindAll(TreeScope.Descendants, by.Condition).Cast<AutomationElement>().ToArray();
	}

	/// <summary>The exception <see cref="Find"/> throws: the locator, the scope and the tree under the scope.</summary>
	public static ElementNotFoundException NotFound(AutomationElement scope, By by) =>
		new ElementNotFoundException(
			$"No element matches {by.Description} within {GtkThread.Invoke(() => Label(scope))}. "
			+ "Automation tree under the scope:" + Environment.NewLine + TreeDump.Of(scope));

	static string Label(AutomationElement scope)
	{
		var info = scope.Current;
		string name = info.AutomationId.Length > 0 ? info.AutomationId : info.Name;
		string type = info.ControlType.ProgrammaticName.Replace("ControlType.", "");
		return name.Length > 0 ? type + " \"" + name + "\"" : type;
	}
}
