using System.Collections.Generic;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Setters are the trigger's content property: direct <Setter> children or a <Owner.Setters> property element.
		static IEnumerable<XElement> TriggerSetters(XElement trigger, string owner)
		{
			foreach (XElement child in trigger.Elements())
			{
				if (child.Name.LocalName == "Setter")
					yield return child;
				else if (child.Name.LocalName == owner + ".Setters")
					foreach (XElement setter in child.Elements())
						if (setter.Name.LocalName == "Setter") yield return setter;
			}
		}

		// <MultiDataTrigger> with <Condition Binding Value/> children, like a DataTrigger with several bindings.
		void EmitMultiDataTriggerToCollection(
			IndentedWriter w, XElement el, string trigVar, string containerVar, string receiver)
		{
			w.Line($"var {trigVar} = new System.Windows.MultiDataTrigger();");
			foreach (XElement child in el.Elements())
			{
				if (child.Name.LocalName != "MultiDataTrigger.Conditions") continue;
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
			foreach (XElement setter in TriggerSetters(el, "MultiDataTrigger"))
			{
				string p = (string)setter.Attribute("Property") ?? string.Empty;
				string vExpr = SetterValueExpression(w, setter, receiver);
				w.Line($"{trigVar}.Setters.Add(new System.Windows.Setter {{ " +
					$"PropertyName = \"{EscapeString(p)}\", Value = {vExpr} }});");
			}
			w.Line($"{containerVar}.Triggers.Add({trigVar});");
		}
	}
}
