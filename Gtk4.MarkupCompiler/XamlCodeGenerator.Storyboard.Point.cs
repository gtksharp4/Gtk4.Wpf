namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string PointAnimationType = "System.Windows.Media.Animation.PointAnimation";

		// PointAnimation From/To/By: Point? via the converter.
		static string PointAnimValueExpression(string val) =>
			$"(System.Windows.Point?)" +
			$"System.Windows.Markup.XamlValueConverter.ConvertByType(" +
			$"typeof(System.Windows.Point), \"{EscapeString(val)}\")";
	}
}
