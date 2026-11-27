using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <TextBlock.Text><PriorityBinding FallbackValue="x"><Binding Path="A"/>...</PriorityBinding></TextBlock.Text>
		void EmitPriorityBindingInline(
			IndentedWriter w, XElement pbEl, string pbVar, ref int counter)
		{
			w.Line($"var {pbVar} = new System.Windows.Data.PriorityBinding();");
			foreach (XAttribute a in pbEl.Attributes().Where(IsPlainAttribute))
			{
				string n = a.Name.LocalName;
				if (n == "FallbackValue" || n == "StringFormat" || n == "TargetNullValue")
					w.Line($"{pbVar}.{n} = \"{EscapeString(a.Value)}\";");
				else NoteDropped(a, "attribute " + n + " on PriorityBinding");
			}
			foreach (XElement child in pbEl.Elements())
			{
				if (child.Name.LocalName != "Binding")
				{
					NoteDropped(child, "element " + child.Name.LocalName + " in PriorityBinding");
					continue;
				}
				string b = $"__b{counter++}";
				EmitBindingElementInline(w, child, b, ref counter);
				w.Line($"{pbVar}.Bindings.Add({b});");
			}
		}
	}
}
