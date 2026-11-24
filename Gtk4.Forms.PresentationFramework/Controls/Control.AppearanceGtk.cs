namespace System.Windows.Controls;

public abstract partial class Control
{
	/// <summary>
	/// Control CSS: background, border and text. <c>color</c>/<c>font-*</c> go on the control's own node
	/// and reach its inner label nodes by GTK CSS inheritance. Background and border go on the node that
	/// paints the face (see <c>Control.AppearanceGtk.Selectors.cs</c>); an explicit brush overrides the
	/// theme's <c>background-image</c>/<c>box-shadow</c> there, as a WPF template binding would.
	/// <para>
	/// Gap: <c>Image</c>/<c>Visual</c>/<c>DrawingBrush</c> backgrounds are store-only (no CSS form).
	/// </para>
	/// </summary>
	internal override string BuildAppearanceCss()
	{
		string text = ElementCssStyle.Text(this);
		return FaceIsRoot ? ElementCssStyle.Join(FaceDeclarations(), text) : text;
	}

	/// <summary>The Aero2 state rules; empty for controls on the theme.</summary>
	internal override string BuildAppearanceRules() => StateRules();

	/// <inheritdoc/>
	internal override Gtk.Widget? AppearanceChildTarget => ChildFace(Widget) == null ? null : Widget.FirstChild;

	/// <inheritdoc/>
	internal override string BuildAppearanceChildRules() => FaceRule();
}
