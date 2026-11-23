using System.Globalization;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// A literal ClrValueExpression emits as is: an integer or a dotted decimal.
		static bool IsPlainNumber(string val)
			=> long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
				|| (val.Contains(".") && double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
	}
}
