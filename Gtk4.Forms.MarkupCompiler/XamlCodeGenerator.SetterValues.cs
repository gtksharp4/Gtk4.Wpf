using System;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Setter value variables live in a separate range so they never collide with __eN counters.
		int _setterCounter = 10000;
		XNamespace _xNs;

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
					: $"{receiver}.FindResource(\"{k}\")";
			}
			if (IsXStatic(raw, out string staticExpr, _prefixMap)) return staticExpr;
			return $"\"{EscapeString(raw)}\"";
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
		/// Emits <c>new EventSetter(Owner.XEvent, Delegate.CreateDelegate(Owner.XEvent.HandlerType, this, "H"))</c>
		/// into <paramref name="setters"/>. The owner is the style TargetType unless the event is
		/// qualified (<c>Owner.Event</c>). The event's own delegate type is used and the 3-argument
		/// overload binds non-public handlers, as WPF does.
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

			string ownerFq = ResolveTargetType(owner);
			string handled = (string)el.Attribute("HandledEventsToo");
			string init = string.Equals(handled, "true", StringComparison.OrdinalIgnoreCase)
				? " { HandledEventsToo = true }" : string.Empty;
			w.Line($"{setters}.Add(new System.Windows.EventSetter({ownerFq}.{ev}Event, " +
				$"System.Delegate.CreateDelegate({ownerFq}.{ev}Event.HandlerType, this, " +
				$"\"{EscapeString(handler)}\")){init});");
		}
	}
}
