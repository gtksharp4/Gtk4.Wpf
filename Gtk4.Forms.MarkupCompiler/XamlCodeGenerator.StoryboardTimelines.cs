using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// The timeline children of <paramref name="parentEl"/> (a Storyboard or ParallelTimeline), added to
		// <paramref name="parentVar"/>; Storyboard.TargetName/TargetProperty go through the root storyboard.
		void EmitTimelineChildren(
			IndentedWriter w, XElement parentEl, XNamespace xNs, string parentVar, string sbVar, ref int counter)
		{
			foreach (XElement child in parentEl.Elements())
			{
				string childLocal = child.Name.LocalName;
				if (childLocal.IndexOf('.') >= 0) continue; // skip property elements

				string animFqType = WpfXmlnsMap.Resolve(childLocal);
				if (animFqType == null) continue;

				string animVar = $"__anim{counter++}";
				w.Line($"var {animVar} = new {animFqType}();");
				EmitTimelineName(w, child, xNs, animVar);

				// Attributes on the animation element.
				foreach (XAttribute attr in child.Attributes())
				{
					if (attr.IsNamespaceDeclaration) continue;

					string attrName = attr.Name.LocalName;
					string attrNs = attr.Name.NamespaceName;
					string val = attr.Value;

					// Skip x: namespace (e.g. x:Key).
					if (attrNs == WpfXmlnsMap.XNs || attrNs == WpfXmlnsMap.XNs2006) continue;

					// Attached property: "Owner.Prop" syntax.
					int dot = attrName.IndexOf('.');
					if (dot > 0)
					{
						string ownerLocalName = attrName.Substring(0, dot);
						string attachedProp = attrName.Substring(dot + 1);

						// Storyboard.TargetName and Storyboard.TargetProperty are the main ones.
						if (ownerLocalName == "Storyboard")
						{
							if (attachedProp == "TargetName")
							{
								w.Line($"{sbVar}.SetTargetName({animVar}, \"{EscapeString(val)}\");");
							}
							else if (attachedProp == "TargetProperty")
							{
								// Store as a property name string; resolved lazily at Begin() time.
								w.Line($"{sbVar}.SetTargetPropertyName({animVar}, \"{EscapeString(val)}\");");
							}
						}
						continue;
					}

					if (TryEmitAnimationMarkupAttribute(w, child, animVar, animFqType, attr)) continue;

					// CLR properties (From, To, By, EasingFunction) on animation types.
					if (_animClrProperties.Contains(attrName))
					{
						w.Line($"{animVar}.{attrName} = {AnimValueExpression(animFqType, attrName, val)};");
						continue;
					}

					// Timeline base properties (Duration, RepeatBehavior, AutoReverse, BeginTime, FillBehavior).
					EmitTimelineProperty(w, animVar, animFqType, attrName, val);
				}

				EmitAnimationChildren(w, child, xNs, animVar, ref counter);
				if (child.Name.LocalName == "ParallelTimeline" || child.Name.LocalName == "Storyboard")
					EmitTimelineChildren(w, child, xNs, animVar, sbVar, ref counter);
				w.Line($"{parentVar}.Children.Add({animVar});");
			}
		}
	}
}
