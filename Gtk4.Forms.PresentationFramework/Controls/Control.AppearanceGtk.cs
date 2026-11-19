namespace System.Windows.Controls;

public abstract partial class Control
{
	/// <summary>
	/// Control CSS: background, border and text. <c>color</c>/<c>font-*</c> go on the control's own node
	/// and reach its inner label nodes by GTK CSS inheritance.
	/// <para>
	/// Gap: <c>Image</c>/<c>Visual</c>/<c>DrawingBrush</c> backgrounds are store-only (no CSS form).
	/// </para>
	/// </summary>
	internal override string BuildAppearanceCss() => ElementCssStyle.Join(
		ElementCssStyle.Background(this), ElementCssStyle.Border(this), ElementCssStyle.Text(this));
}
