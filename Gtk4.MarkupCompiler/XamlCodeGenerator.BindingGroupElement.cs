using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Emits <c>&lt;X.BindingGroup&gt;&lt;BindingGroup .../&gt;&lt;/X.BindingGroup&gt;</c> (and
		/// <c>ItemsControl.ItemBindingGroup</c>): a group with its name, flags and validation rules.
		/// </summary>
		void EmitBindingGroupElement(
			IndentedWriter w, XElement group, string parentVar, string propName, ref int counter)
		{
			string gVar = "__bg" + counter++;
			w.Line("var " + gVar + " = new System.Windows.Data.BindingGroup();");
			foreach (XAttribute attr in group.Attributes())
			{
				if (attr.IsNamespaceDeclaration) continue;
				string v = attr.Value.Trim();
				switch (attr.Name.LocalName)
				{
					case "Name":
						w.Line(gVar + ".Name = \"" + EscapeString(attr.Value) + "\";");
						break;
					case "NotifyOnValidationError":
					case "ValidatesOnNotifyDataError":
					case "CanRestoreValues":
					case "SharesProposedValues":
						if (bool.TryParse(v, out bool flag))
							w.Line(gVar + "." + attr.Name.LocalName + " = " + (flag ? "true" : "false") + ";");
						break;
				}
			}
			foreach (XElement prop in group.Elements())
			{
				if (prop.Name.LocalName == "BindingGroup.ValidationRules")
					EmitValidationRules(w, prop, gVar + ".ValidationRules", ref counter);
				else
					NoteDropped(prop, "element " + prop.Name.LocalName + " in BindingGroup");
			}
			w.Line(parentVar + "." + propName + " = " + gVar + ";");
		}
	}
}
