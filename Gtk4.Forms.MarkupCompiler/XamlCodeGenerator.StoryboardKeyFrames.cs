using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// A markup extension on an animation attribute: {Binding} binds the dependency property, {StaticResource}
		// resolves once, {DynamicResource} has no target (an animation has no SetResourceReference) and is skipped.
		bool TryEmitAnimationMarkupAttribute(
			IndentedWriter w, XElement el, string animVar, string animFqType, XAttribute attr)
		{
			string name = attr.Name.LocalName;
			string val = attr.Value;
			if (!val.TrimStart().StartsWith("{", StringComparison.Ordinal)
				|| val.TrimStart().StartsWith("{}", StringComparison.Ordinal))
				return false;
			bool hasDp = _resolver.HasReferences && IsKnownDp(animFqType, name);
			if (IsBindingExpression(val, out string binding) && hasDp)
				w.Line(SetBindingLine(animVar, animFqType, $"{animFqType}.{name}Property", binding));
			else if (IsStaticResourceValue(val, out string key) && hasDp)
				w.Line($"{animVar}.SetValue({animFqType}.{name}Property, " +
					$"{StaticResourceLookup("this", EscapeString(key))});");
			else
				NoteDropped(attr, $"{val.Trim()} on {el.Name.LocalName}.{name}");
			return true;
		}

		// Key frames (and the x:Name of an animation or storyboard) of an animation element.
		void EmitAnimationChildren(
			IndentedWriter w, XElement anim, XNamespace xNs, string animVar, ref int counter)
		{
			if (!anim.Name.LocalName.EndsWith("UsingKeyFrames", StringComparison.Ordinal)) return;
			var frames = new List<XElement>();
			foreach (XElement c in anim.Elements())
			{
				if (c.Name.LocalName.IndexOf('.') >= 0) frames.AddRange(c.Elements());
				else frames.Add(c);
			}
			foreach (XElement frame in frames)
			{
				string fq = ResolveType(frame) ?? WpfXmlnsMap.Resolve(frame.Name.LocalName);
				if (fq == null || !_resolver.TypeExists(fq)
					|| !frame.Name.LocalName.EndsWith("KeyFrame", StringComparison.Ordinal))
				{
					NoteDropped(frame, "element " + frame.Name.LocalName + " in " + anim.Name.LocalName);
					continue;
				}
				string v = $"__kf{counter++}";
				EmitElement(w, frame, xNs, v, fq, ref counter);
				w.Line($"{animVar}.KeyFrames.Add({v});");
			}
		}

		void EmitTimelineName(IndentedWriter w, XElement el, XNamespace xNs, string varName)
		{
			string name = NameOf(el, xNs);
			if (name == null || !_fieldElements.Contains(el)) return;
			w.Line($"this.{name} = {varName};");
			EmitNameRegistration(w, varName, name);
		}
	}
}
