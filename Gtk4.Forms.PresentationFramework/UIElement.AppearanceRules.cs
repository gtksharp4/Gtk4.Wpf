namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>Extra appearance rules on the widget's own node (state rules); <c>&amp;</c> is its selector.</summary>
	internal virtual string BuildAppearanceRules() => string.Empty;

	/// <summary>The inner widget whose CSS node paints the face, or <c>null</c> when it is the widget itself.</summary>
	internal virtual Gtk.Widget? AppearanceChildTarget => null;

	/// <summary>
	/// Rules for <see cref="AppearanceChildTarget"/>; plain selectors, scoped by the provider's widget.
	/// </summary>
	internal virtual string BuildAppearanceChildRules() => string.Empty;

	/// <summary>A second inner widget whose CSS node paints part of the face, or <c>null</c>.</summary>
	internal virtual Gtk.Widget? AppearanceExtraTarget => null;

	/// <summary>Rules for <see cref="AppearanceExtraTarget"/>.</summary>
	internal virtual string BuildAppearanceExtraRules() => string.Empty;

	/// <summary>The extra appearance rules last pushed to the widget; for tests.</summary>
	internal string AppearanceRules => _appearance?.Rules ?? string.Empty;

	/// <summary>The child-node rules last pushed; for tests.</summary>
	internal string AppearanceChildRules => _appearance?.ChildRules ?? string.Empty;

	/// <summary>The second child-node rules last pushed; for tests.</summary>
	internal string AppearanceExtraRules => _appearance?.ExtraRules ?? string.Empty;

	/// <summary>Parse errors of the child-node rules; for tests.</summary>
	internal int AppearanceChildParseErrors => _appearance?.ChildParseErrors ?? 0;
}
