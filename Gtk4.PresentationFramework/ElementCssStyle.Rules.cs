namespace System.Windows;

internal sealed partial class ElementCssStyle
{
	readonly ChildStyle _child = new ChildStyle();
	readonly ChildStyle _extra = new ChildStyle();

	/// <summary>
	/// Extra rules loaded next to the main block (state pseudo-classes). <c>&amp;</c> stands for the widget's
	/// unique <c>#name</c> selector, so a rule can never leak to another widget.
	/// </summary>
	internal string Rules { get; private set; } = string.Empty;

	/// <summary>Rules last loaded on the child widget (see <see cref="UpdateChild"/>).</summary>
	internal string ChildRules => _child.Rules;

	/// <summary>CSS parse errors GTK reported for the child rules.</summary>
	internal int ChildParseErrors => _child.ParseErrors;

	/// <summary>Rules last loaded on the second child widget (see <see cref="UpdateExtra"/>).</summary>
	internal string ExtraRules => _extra.Rules;

	/// <summary>CSS parse errors GTK reported for the second child's rules.</summary>
	internal int ExtraParseErrors => _extra.ParseErrors;

	static string ExpandRules(string rules, string name) =>
		rules.Length == 0 ? string.Empty : " " + rules.Replace("&", "#" + name);

	/// <summary>
	/// A provider on the widget reaches only its own CSS node in GTK 4 (measured: a bare <c>check</c> rule on the
	/// CheckButton did nothing), so a rule for an inner node, such as the check indicator or the dropdown's
	/// button, rides on a provider attached to that child widget.
	/// </summary>
	internal void UpdateChild(Gtk.Widget? child, string rules) => _child.Update(child, rules);

	/// <summary>The same for a second inner widget (an editable ComboBox paints its entry and its button).</summary>
	internal void UpdateExtra(Gtk.Widget? child, string rules) => _extra.Update(child, rules);

	sealed class ChildStyle
	{
		const uint ChildPriority = 600;
		Gtk.CssProvider? _provider;
		Gtk.Widget? _target;

		internal string Rules { get; private set; } = string.Empty;

		internal int ParseErrors { get; private set; }

		internal void Update(Gtk.Widget? child, string rules)
		{
			if (child == null || rules.Length == 0)
			{
				Detach();
				Rules = string.Empty;
				return;
			}
			if (_provider != null && ReferenceEquals(child, _target) && rules == Rules) return;
			if (_provider == null || !ReferenceEquals(child, _target))
			{
				Detach();
				_provider = new Gtk.CssProvider();
				_provider.ParsingError += (_, __) => ParseErrors++;
				child.StyleContext.AddProvider(_provider, ChildPriority);
				_target = child;
			}
			Rules = rules;
			ParseErrors = 0;
			_provider.LoadFromData(rules);
		}

		void Detach()
		{
			if (_provider != null && _target != null) _target.StyleContext.RemoveProvider(_provider);
			_provider = null;
			_target = null;
		}
	}
}
