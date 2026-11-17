namespace System.Windows.Media.Effects;

/// <summary>
/// Abstract base for bitmap effects applied via <see cref="System.Windows.UIElement.Effect"/>.
/// <para>
/// <b>GTK application:</b> each concrete effect converts to a CSS <c>filter:</c> value via
/// <see cref="EffectCss.BuildFilter"/>. The CSS is pushed to the widget through a per-widget
/// <c>Gtk.CssProvider</c> when the widget is realized (or immediately if already realized).
/// The object model (DP values) is always available regardless of GTK availability.
/// </para>
/// </summary>
public abstract class Effect
{
}
