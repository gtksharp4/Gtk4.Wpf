using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string GroupStyleType = "System.Windows.Controls.GroupStyle";

		// Non-element objects the item views are made of: built by EmitElement and attached to their owner.
		static bool IsItemViewObjectType(string fq)
		{
			switch (fq?.Replace("global::", ""))
			{
				case "System.Windows.Controls.GridView":
				case "System.Windows.Controls.GridViewColumn":
				case GroupStyleType:
				case "System.Windows.Data.CompositeCollection":
				case "System.Windows.Data.CollectionContainer":
					return true;
				default:
					return false;
			}
		}

		// <x:Static Member="GroupStyle.Default"/> as an item of a GroupStyle collection.
		bool IsGroupStyleDefault(XElement c, XNamespace xNs, System.Type itemType)
		{
			if (itemType?.FullName != GroupStyleType || c.Name != xNs + "Static") return false;
			string member = (string)c.Attribute("Member");
			return member == "GroupStyle.Default"
				|| (member != null && member.EndsWith(":GroupStyle.Default", System.StringComparison.Ordinal));
		}
	}
}
