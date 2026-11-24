using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// <MultiDataTrigger> with <Condition Binding Value/> children, like a DataTrigger with several bindings.
		void EmitMultiDataTriggerToCollection(
			IndentedWriter w, XElement el, string trigVar, string containerVar, string receiver)
		{
			w.Line($"var {trigVar} = new System.Windows.MultiDataTrigger();");
			foreach (XElement child in el.Elements())
			{
				if (child.Name.LocalName == "MultiDataTrigger.Conditions")
				{
					foreach (XElement cond in child.Elements())
					{
						if (cond.Name.LocalName != "Condition") continue;
						string bindingAttr = (string)cond.Attribute("Binding") ?? string.Empty;
						string v = (string)cond.Attribute("Value") ?? string.Empty;
						string bindingExpr = IsBindingExpression(bindingAttr, out string bInit) ? bInit : "null";
						w.Line($"{trigVar}.Conditions.Add(new System.Windows.Condition {{ " +
							$"Binding = {bindingExpr}, Value = \"{EscapeString(v)}\" }});");
					}
				}
				else if (child.Name.LocalName == "Setter")
				{
					string p = (string)child.Attribute("Property") ?? string.Empty;
					string vExpr = SetterValueExpression(w, child, receiver);
					w.Line($"{trigVar}.Setters.Add(new System.Windows.Setter {{ " +
						$"PropertyName = \"{EscapeString(p)}\", Value = {vExpr} }});");
				}
			}
			w.Line($"{containerVar}.Triggers.Add({trigVar});");
		}
	}
}
