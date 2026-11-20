using System;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Setter value variables live in a separate range so they never collide with __eN counters.
		int _setterCounter = 10000;
		XNamespace _xNs;

		// Element variables enclosing the point being emitted, outermost first (the root is "this").
		readonly System.Collections.Generic.List<string> _scopeVars = new System.Collections.Generic.List<string>();
		string _classFqName;

		// The scope list of the Generate call running on this thread; static emit helpers read it.
		[ThreadStatic]
		static System.Collections.Generic.List<string> _activeScopeVars;

		/// <summary>
		/// Returns the C# expression for a <c>&lt;Setter&gt;</c> value, emitting any helper locals
		/// first. The <c>Value</c> attribute wins over a <c>&lt;Setter.Value&gt;</c> property element.
		/// <paramref name="receiver"/> is the object whose <c>FindResource</c> resolves static resources.
		/// </summary>
		string SetterValueExpression(IndentedWriter w, XElement setterEl, string receiver)
		{
			string attr = (string)setterEl.Attribute("Value");
			if (attr != null) return SetterAttributeValue(attr, receiver);

			foreach (XElement pe in setterEl.Elements())
				if (pe.Name.LocalName == "Setter.Value")
					return EmitSetterValueElement(w, pe, receiver);
			return "null";
		}

		string SetterAttributeValue(string raw, string receiver)
		{
			if (IsXNull(raw)) return "null";
			if (IsTemplateBindingExpression(raw, out string tb)) return tb;
			if (IsBindingExpression(raw, out string binding)) return binding;
			if (IsStaticOrDynamicResource(raw, out string key))
			{
				string k = EscapeString(key);
				return raw.TrimStart().StartsWith("{DynamicResource", StringComparison.Ordinal)
					? $"new System.Windows.Markup.DynamicResourceExtension(\"{k}\")"
					: StaticResourceLookup(receiver, k);
			}
			if (IsXType(raw, out string typeExpr, _prefixMap, ProbePresentationType)) return typeExpr;
			if (IsXStatic(raw, out string staticExpr, _prefixMap, ProbePresentationType)) return staticExpr;
			return $"\"{EscapeString(raw)}\"";
		}

		/// <summary>
		/// Setter values are emitted before the owner is attached, so its own chain cannot see the enclosing
		/// scopes. WPF resolves against the parse-time scope stack: every enclosing element's resources from the
		/// innermost out, then the root, then the application.
		/// </summary>
		static string StaticResourceLookup(string receiver, string escapedKey)
		{
			var scopes = new System.Collections.Generic.List<string>();
			var vars = _activeScopeVars ?? new System.Collections.Generic.List<string>();
			for (int i = vars.Count - 1; i >= 0; i--)
				scopes.Add(vars[i]);
			if (receiver != "this" && !scopes.Contains(receiver))
				scopes.Insert(0, receiver);
			if (scopes.Count == 0)
				return $"this.FindResource(\"{escapedKey}\")";
			return $"System.Windows.Markup.StaticResourceScope.Find(\"{escapedKey}\", this, " +
				string.Join(", ", scopes) + ")";
		}

		string EmitSetterValueElement(IndentedWriter w, XElement valueEl, string receiver)
		{
			XNamespace xNs = _xNs ?? XNamespace.Get(WpfXmlnsMap.XNs2006);
			foreach (XElement child in valueEl.Elements())
			{
				return EmitSetterChild(w, child, xNs, ref _setterCounter);
			}
			string text = valueEl.Value.Trim();
			return text.Length == 0 ? "null" : SetterAttributeValue(text, receiver);
		}

		string EmitSetterChild(IndentedWriter w, XElement child, XNamespace xNs, ref int counter)
		{
			string v = $"__sv{counter++}";
			switch (child.Name.LocalName)
			{
				case "ControlTemplate": EmitControlTemplateInline(w, child, xNs, v, ref counter); return v;
				case "DataTemplate": EmitDataTemplateInline(w, child, xNs, v, ref counter); return v;
				case "MultiBinding": EmitMultiBindingInline(w, child, xNs, v, ref counter); return v;
			}
			string fq = ResolveType(child);
			if (fq == null) return "null";
			EmitElement(w, child, xNs, v, fq, ref counter);
			return v;
		}

		/// <summary>
		/// Emits <c>new EventSetter(Owner.XEvent, Delegate.CreateDelegate(Owner.XEvent.HandlerType, this,
		/// typeof(Class).GetMethod("H", ...)))</c> into <paramref name="setters"/>. The owner is the style TargetType
		/// unless the event is qualified. Binding against the x:Class (not GetType()) keeps private handlers reachable
		/// from derived classes. The MethodInfo overload binds non-public handlers and allows contravariant
		/// parameters (<c>(object, RoutedEventArgs)</c> on MouseDown), as C# method-group conversion does in WPF.
		/// </summary>
		void EmitEventSetter(IndentedWriter w, XElement el, string setters, string targetTypeAttr)
		{
			string ev = (string)el.Attribute("Event");
			string handler = (string)el.Attribute("Handler");
			if (ev == null || handler == null) return;

			string owner = targetTypeAttr;
			int dot = ev.LastIndexOf('.');
			if (dot >= 0)
			{
				owner = ev.Substring(0, dot);
				ev = ev.Substring(dot + 1);
			}
			if (owner == null) return;

			string ownerFq = ResolveTypeRefName(owner) ?? owner;
			string handled = (string)el.Attribute("HandledEventsToo");
			string init = string.Equals(handled, "true", StringComparison.OrdinalIgnoreCase)
				? " { HandledEventsToo = true }" : string.Empty;
			w.Line($"{setters}.Add(new System.Windows.EventSetter({ownerFq}.{ev}Event, " +
				$"System.Delegate.CreateDelegate({ownerFq}.{ev}Event.HandlerType, this, " +
				$"typeof({_classFqName}).GetMethod(\"{EscapeString(handler)}\", " +
				$"System.Reflection.BindingFlags.Instance | " +
				$"System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))){init});");
		}
	}
}
