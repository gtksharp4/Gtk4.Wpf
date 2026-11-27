using System;
using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string KeyBindingType = "System.Windows.Input.KeyBinding";
		const string MouseBindingType = "System.Windows.Input.MouseBinding";

		// <Window.InputBindings><KeyBinding Command="..." Key="F4" Modifiers="Alt"/> or Gesture="CTRL+R", and
		// <MouseBinding Command="..." MouseAction="LeftDoubleClick"/>. The layer's bindings take command and
		// gesture in the constructor, so each one is emitted as an object-creation expression. Anything else
		// (a Command property element, a binding as the command, an unknown gesture) is left to the skip message.
		bool TryEmitInputBindings(IndentedWriter w, XElement pe, string parentVar)
		{
			if (!_resolver.HasReferences || !_resolver.TypeExists(KeyBindingType) || !pe.Elements().Any()
				|| pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			var lines = new System.Collections.Generic.List<string>();
			foreach (XElement e in pe.Elements())
			{
				string expr = InputBindingExpression(e);
				if (expr == null) return false;
				lines.Add($"{parentVar}.InputBindings.Add({expr});");
			}
			foreach (string l in lines) w.Line(l);
			return true;
		}

		string InputBindingExpression(XElement e)
		{
			string kind = e.Name.LocalName;
			string cmd = CommandBindingCommand(e);
			if (cmd == null || e.HasElements || (kind != "KeyBinding" && kind != "MouseBinding")) return null;
			string parameter = null;
			foreach (XAttribute a in e.Attributes().Where(IsPlainAttribute))
			{
				string n = a.Name.LocalName;
				if (n == "Command" || n == "Key" || n == "Modifiers" || n == "MouseAction" || n == "Gesture") continue;
				if (n == "CommandParameter" && a.Value.IndexOf('{') < 0) parameter = a.Value;
				else return null;
			}
			string key = (string)e.Attribute("Key");
			string modifiers = (string)e.Attribute("Modifiers");
			string gestureText = (string)e.Attribute("Gesture");
			string ctor;
			if (kind == "KeyBinding")
			{
				if (gestureText != null && key == null && modifiers == null)
				{
					if (!TryParseKeyGesture(gestureText, out key, out modifiers)) return null;
				}
				if (key == null || key.IndexOf('{') >= 0 || !TryParseModifiers(modifiers, out string mods)) return null;
				string gesture = $"(global::System.Windows.Input.Key)global::System.Enum.Parse(" +
					$"typeof(global::System.Windows.Input.Key), \"{EscapeString(key.Trim())}\", true), {mods}";
				ctor = $"new global::{KeyBindingType}({cmd}, {gesture})";
			}
			else
			{
				string action = (string)e.Attribute("MouseAction");
				if (action == null || gestureText != null || action.IndexOf('{') >= 0
					|| !TryParseModifiers(modifiers, out string mods)) return null;
				ctor = $"new global::{MouseBindingType}({cmd}, new global::System.Windows.Input.MouseGesture(" +
					$"(global::System.Windows.Input.MouseAction)global::System.Enum.Parse(" +
					$"typeof(global::System.Windows.Input.MouseAction), \"{EscapeString(action.Trim())}\", true), {mods}))";
			}
			return parameter == null ? ctor : $"{ctor} {{ CommandParameter = \"{EscapeString(parameter)}\" }}";
		}

		// "CTRL+R" is the last token as the key and the rest as modifiers (WPF KeyGestureConverter).
		static bool TryParseKeyGesture(string text, out string key, out string modifiers)
		{
			key = null;
			modifiers = null;
			string[] parts = text.Split('+');
			if (parts.Length == 0 || parts.Any(p => p.Trim().Length == 0)) return false;
			key = parts[parts.Length - 1].Trim();
			modifiers = parts.Length > 1 ? string.Join("+", parts.Take(parts.Length - 1)) : null;
			return true;
		}

		// The ModifierKeys expression for "Alt", "Ctrl+Shift" (WPF ModifierKeysConverter), "None" when absent.
		static bool TryParseModifiers(string text, out string expr)
		{
			expr = "global::System.Windows.Input.ModifierKeys.None";
			if (string.IsNullOrWhiteSpace(text)) return true;
			var names = new System.Collections.Generic.List<string>();
			foreach (string p in text.Split('+'))
			{
				switch (p.Trim().ToLowerInvariant())
				{
					case "ctrl": case "control": names.Add("Control"); break;
					case "alt": names.Add("Alt"); break;
					case "shift": names.Add("Shift"); break;
					case "windows": case "win": names.Add("Windows"); break;
					case "none": break;
					default: return false;
				}
			}
			if (names.Count > 0)
				expr = string.Join(" | ", names.Select(n => "global::System.Windows.Input.ModifierKeys." + n));
			return true;
		}
	}
}
