namespace System.Windows;

internal sealed partial class ElementCssStyle
{
	const uint ChildPriority = 600;
	Gtk.CssProvider? _childProvider;
	Gtk.Widget? _childTarget;

	/// <summary>
	/// Extra rules loaded next to the main block (state pseudo-classes). <c>&amp;</c> stands for the widget's
	/// unique <c>#name</c> selector, so a rule can never leak to another widget.
	/// </summary>
	internal string Rules { get; private set; } = string.Empty;

	/// <summary>Rules last loaded on the child widget (see <see cref="UpdateChild"/>).</summary>
	internal string ChildRules { get; private set; } = string.Empty;

	/// <summary>CSS parse errors GTK reported for the child rules.</summary>
	internal int ChildParseErrors { get; private set; }

	static string ExpandRules(string rules, string name) =>
		rules.Length == 0 ? string.Empty : " " + rules.Replace("&", "#" + name);

	/// <summary>
	/// A provider on the widget reaches only its own CSS node in GTK 4 (measured: a bare <c>check</c> rule on the
	/// CheckButton did nothing), so a rule for an inner node, such as the check indicator or the dropdown's
	/// button, rides on a provider attached to that child widget.
	/// </summary>
	internal void UpdateChild(Gtk.Widget? child, string rules)
	{
		if (child == null || rules.Length == 0)
		{
			if (_childProvider != null && _childTarget != null) _childTarget.StyleContext.RemoveProvider(_childProvider);
			_childProvider = null;
			_childTarget = null;
			ChildRules = string.Empty;
			return;
		}
		if (_childProvider != null && ReferenceEquals(child, _childTarget) && rules == ChildRules) return;
		if (_childProvider == null || !ReferenceEquals(child, _childTarget))
		{
			if (_childProvider != null && _childTarget != null) _childTarget.StyleContext.RemoveProvider(_childProvider);
			_childProvider = new Gtk.CssProvider();
			_childProvider.ParsingError += (_, __) => ChildParseErrors++;
			child.StyleContext.AddProvider(_childProvider, ChildPriority);
			_childTarget = child;
		}
		ChildRules = rules;
		ChildParseErrors = 0;
		_childProvider.LoadFromData(rules);
	}
}
