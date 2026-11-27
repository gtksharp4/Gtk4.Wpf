using System;
using System.Linq;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string KeyBindingType = "System.Windows.Input.KeyBinding";
		const string MouseBindingType = "System.Windows.Input.MouseBinding";

		const string KeyType = "System.Windows.Input.Key";
		const string MouseActionType = "System.Windows.Input.MouseAction";

		static readonly System.Collections.Generic.Dictionary<string, string> KeyAliases =
			new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["CTRL"] = "LeftCtrl", ["CONTROL"] = "LeftCtrl", ["ALT"] = "LeftAlt", ["SHIFT"] = "LeftShift",
			["ESC"] = "Escape", ["PGUP"] = "PageUp", ["PGDN"] = "PageDown", ["INS"] = "Insert",
			["DEL"] = "Delete", ["WIN"] = "LWin", ["WINDOWS"] = "LWin", ["BACKSPACE"] = "Back",
			["BKSP"] = "Back", ["BS"] = "Back", ["ENTER"] = "Return", ["PLUS"] = "OemPlus",
			["MINUS"] = "OemMinus", ["COMMA"] = "OemComma", ["PERIOD"] = "OemPeriod",
			["SEMICOLON"] = "OemSemicolon", ["QUESTION"] = "OemQuestion", ["TILDE"] = "OemTilde",
			["OPENBRACKETS"] = "OemOpenBrackets", ["CLOSEBRACKETS"] = "OemCloseBrackets", ["PIPE"] = "OemPipe",
			["QUOTES"] = "OemQuotes", ["BACKSLASH"] = "OemBackslash", ["BREAK"] = "Cancel",
			["PRINTSCREEN"] = "PrintScreen", ["APPS"] = "Apps", ["PAUSE"] = "Pause", ["SCROLLLOCK"] = "Scroll",
			["NUMLOCK"] = "NumLock",
		};

		// The member of enum fqEnum named by text (case-insensitive), or null when the enum or member is unknown.
		string EnumMember(string fqEnum, string text)
		{
			string[] names = _resolver.GetEnumMemberNames(fqEnum);
			return names?.FirstOrDefault(n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase));
		}

		// Mirrors the layer's KeyConverter at generation time: a digit is D0-D9, a letter is itself, then the
		// alias table, then an enum name. Null when nothing matches, so the element is reported as skipped.
		string ResolveKeyName(string text)
		{
			text = text.Trim();
			if (text.Length == 1 && text[0] >= '0' && text[0] <= '9') return EnumMember(KeyType, "D" + text);
			if (text.Length == 1 && char.IsLetter(text[0])) return EnumMember(KeyType, text);
			if (KeyAliases.TryGetValue(text, out string alias)) return EnumMember(KeyType, alias);
			return EnumMember(KeyType, text);
		}

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
				string keyName = ResolveKeyName(key);
				if (keyName == null) return null;
				string gesture = $"global::System.Windows.Input.Key.{keyName}, {mods}";
				ctor = $"new global::{KeyBindingType}({cmd}, {gesture})";
			}
			else
			{
				string action = (string)e.Attribute("MouseAction");
				if (action == null || gestureText != null || action.IndexOf('{') >= 0
					|| !TryParseModifiers(modifiers, out string mods)) return null;
				string actionName = EnumMember(MouseActionType, action.Trim());
				if (actionName == null) return null;
				ctor = $"new global::{MouseBindingType}({cmd}, new global::System.Windows.Input.MouseGesture(" +
					$"global::{MouseActionType}.{actionName}, {mods}))";
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
