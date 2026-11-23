using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string CommandBindingType = "System.Windows.Input.CommandBinding";

		// <Window.CommandBindings><CommandBinding Command="..." Executed="H" CanExecute="H2"/>: handlers are
		// code-behind methods wired with +=.
		bool TryEmitCommandBindings(IndentedWriter w, XElement pe, string parentVar)
		{
			if (!OnlyElements(pe, "CommandBinding") || !_resolver.TypeExists(CommandBindingType)) return false;
			if (pe.Elements().Any(e => CommandBindingCommand(e) == null)) return false;
			foreach (XElement cb in pe.Elements())
			{
				string v = $"__cb{_sweep4Counter++}";
				w.Line($"var {v} = new {CommandBindingType}({CommandBindingCommand(cb)});");
				foreach (XAttribute a in cb.Attributes().Where(IsPlainAttribute))
					if (a.Name.LocalName != "Command" && _resolver.IsEvent(CommandBindingType, a.Name.LocalName))
						w.Line($"{v}.{a.Name.LocalName} += this.{a.Value};");
				w.Line($"{parentVar}.CommandBindings.Add({v});");
			}
			return true;
		}

		int _sweep4Counter;

		// The C# expression for the command of a CommandBinding, or null when it has none.
		string CommandBindingCommand(XElement cb)
		{
			string val = (string)cb.Attribute("Command");
			if (string.IsNullOrWhiteSpace(val)) return null;
			if (IsXStatic(val, out string expr, _prefixMap, ProbePresentationType))
				return $"(System.Windows.Input.ICommand){expr}";
			if (val.IndexOf('{') >= 0) return null;
			if (TryPrefixedCommand(CommandBindingType, "Command", val, out expr)) return expr;
			return $"(System.Windows.Input.ICommand){Converter}.ConvertByType(" +
				$"typeof(System.Windows.Input.ICommand), \"{EscapeString(val.Trim())}\")";
		}
	}
}
