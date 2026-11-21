namespace System.Windows;

/// <summary>
/// Base class for content that is not a visual: flow-document elements. Only what
/// <see cref="FrameworkContentElement"/> needs is present.
/// <para>
/// Gap: WPF's ContentElement also carries the input surface (IsEnabled, Focusable, the mouse/keyboard/stylus
/// routed events, IInputElement) and animation. Flow content is rendered as one Pango label, so there is no
/// per-element hit target to route input to; those members are absent rather than inert.
/// </para>
/// </summary>
public class ContentElement : DependencyObject
{
}
