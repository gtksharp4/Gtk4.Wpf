using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// Storyboard and animation emit routines for <see cref="XamlCodeGenerator"/>.
	/// </summary>
	public partial class XamlCodeGenerator
	{
		// ── Known animation property names (CLR-only, not DP) ────────────────────

		// From/To/By/EasingFunction are CLR properties on animation classes.
		static readonly System.Collections.Generic.HashSet<string> _animClrProperties =
			new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal)
			{
				"From", "To", "By", "EasingFunction",
			};

		// ── Storyboard emit ──────────────────────────────────────────────────────

		/// <summary>
		/// Emits code that constructs a <see cref="System.Windows.Media.Animation.Storyboard"/>
		/// with its child animations and assigns it to
		/// <c><paramref name="ownerVar"/>.Resources["<paramref name="key"/>"]</c>.
		/// Returns the name of the local storyboard variable.
		/// </summary>
		string EmitStoryboardResource(
			IndentedWriter w,
			XElement sbEl,
			XNamespace xNs,
			string ownerVar,
			string key,
			ref int counter)
		{
			string sbVar = EmitStoryboardInline(w, sbEl, xNs, ref counter);
			if (key != null)
				w.Line($"{ownerVar}.Resources[\"{EscapeString(key)}\"] = {sbVar};");
			return sbVar;
		}

		/// <summary>
		/// Emits a local variable for a <c>&lt;Storyboard&gt;</c> element, including
		/// construction, Duration/RepeatBehavior/AutoReverse properties, and all child
		/// animation timelines with their <c>Storyboard.TargetName</c> /
		/// <c>Storyboard.TargetProperty</c> attached properties.
		/// Returns the variable name.
		/// </summary>
		internal string EmitStoryboardInline(
			IndentedWriter w,
			XElement sbEl,
			XNamespace xNs,
			ref int counter)
		{
			string sbVar = $"__sb{counter++}";
			w.Line($"var {sbVar} = new System.Windows.Media.Animation.Storyboard();");

			// Storyboard-level Timeline properties (Duration, RepeatBehavior, AutoReverse, BeginTime).
			EmitTimelineProperties(w, sbEl, sbVar, "System.Windows.Media.Animation.Storyboard");
			EmitTimelineName(w, sbEl, xNs, sbVar);

			// Child animation timelines.
			EmitTimelineChildren(w, sbEl, xNs, sbVar, sbVar, ref counter);

			return sbVar;
		}

		/// <summary>
		/// Emits Timeline base property assignments (Duration, RepeatBehavior, AutoReverse,
		/// BeginTime, FillBehavior) from attributes on <paramref name="el"/> onto
		/// <paramref name="varName"/>.
		/// </summary>
		static void EmitTimelineProperties(IndentedWriter w, XElement el, string varName, string fqType)
		{
			foreach (XAttribute attr in el.Attributes())
			{
				if (attr.IsNamespaceDeclaration) continue;
				string attrNs = attr.Name.NamespaceName;
				if (attrNs == WpfXmlnsMap.XNs || attrNs == WpfXmlnsMap.XNs2006) continue;
				string name = attr.Name.LocalName;
				if (name.IndexOf('.') >= 0) continue; // attached
				EmitTimelineProperty(w, varName, fqType, name, attr.Value);
			}
		}

		/// <summary>
		/// Emits a single Timeline property assignment onto <paramref name="varName"/>.
		/// Duration and RepeatBehavior go through <c>XamlValueConverter</c>;
		/// AutoReverse and BeginTime use inline C# expressions.
		/// </summary>
		static void EmitTimelineProperty(
			IndentedWriter w,
			string varName,
			string fqType,
			string propName,
			string val)
		{
			switch (propName)
			{
				case "Duration":
					w.Line($"{varName}.Duration = (System.Windows.Media.Animation.Duration)" +
						$"System.Windows.Markup.XamlValueConverter.Convert(" +
						$"typeof({fqType}), null, " +
						$"typeof(System.Windows.Media.Animation.Duration), \"{EscapeString(val)}\");");
					break;
				case "RepeatBehavior":
					w.Line($"{varName}.RepeatBehavior = (System.Windows.Media.Animation.RepeatBehavior)" +
						$"System.Windows.Markup.XamlValueConverter.Convert(" +
						$"typeof({fqType}), null, " +
						$"typeof(System.Windows.Media.Animation.RepeatBehavior), \"{EscapeString(val)}\");");
					break;
				case "AutoReverse":
					w.Line($"{varName}.AutoReverse = {(val.ToLowerInvariant() == "true" ? "true" : "false")};");
					break;
				case "BeginTime":
					w.Line($"{varName}.BeginTime = System.TimeSpan.Parse(\"{EscapeString(val)}\");");
					break;
				case "FillBehavior":
					w.Line($"{varName}.FillBehavior = System.Windows.Media.Animation.FillBehavior.{val};");
					break;
			}
		}

		/// <summary>
		/// Returns a C# expression for a CLR animation property value (From/To/By).
		/// For numeric animation types returns a nullable double/Color/Thickness literal;
		/// falls back to a string cast.
		/// </summary>
		static string AnimValueExpression(string animFqType, string propName, string val)
		{
			// From/To/By are nullable on the animation classes.
			// DoubleAnimation: double? → emit numeric literal.
			if (animFqType == "System.Windows.Media.Animation.DoubleAnimation")
			{
				if (double.TryParse(val, System.Globalization.NumberStyles.Float,
					System.Globalization.CultureInfo.InvariantCulture, out _))
					return $"(double?){val}";
			}
			// ColorAnimation: Color? → emit via converter.
			if (animFqType == "System.Windows.Media.Animation.ColorAnimation")
			{
				return $"(System.Windows.Media.Color?)" +
					$"System.Windows.Markup.XamlValueConverter.ConvertByType(" +
					$"typeof(System.Windows.Media.Color), \"{EscapeString(val)}\")";
			}
			// ThicknessAnimation: Thickness? → emit via converter.
			if (animFqType == "System.Windows.Media.Animation.ThicknessAnimation")
			{
				return $"(System.Windows.Thickness?)" +
					$"System.Windows.Markup.XamlValueConverter.ConvertByType(" +
					$"typeof(System.Windows.Thickness), \"{EscapeString(val)}\")";
			}
			if (animFqType == PointAnimationType) return PointAnimValueExpression(val);
			// Fallback: quoted string.
			return $"\"{EscapeString(val)}\"";
		}

		// ── EventTrigger with BeginStoryboard ────────────────────────────────────

		/// <summary>
		/// Emits an EventTrigger variable with any Actions, including
		/// <c>&lt;BeginStoryboard&gt;&lt;Storyboard&gt;…&lt;/Storyboard&gt;&lt;/BeginStoryboard&gt;</c>.
		/// Extends the base EventTrigger emit to handle BeginStoryboard children.
		/// </summary>
		void EmitEventTriggerFull(
			IndentedWriter w,
			XElement el,
			XNamespace xNs,
			string trigVar,
			string containerVar,
			bool isStyleTrigger,
			ref int counter)
		{
			string reAttr = (string)el.Attribute("RoutedEvent") ?? string.Empty;
			string reExpr = "null";
			int dot = reAttr.LastIndexOf('.');
			if (dot > 0)
			{
				string ownerName = reAttr.Substring(0, dot);
				string evName = reAttr.Substring(dot + 1);
				string fqOwner = ownerName.IndexOf(':') > 0
					? ResolveTypeRefName(ownerName) : Emitted(QualifyTypeName(ownerName));
				// A prefixed owner is a user type: reference its static RoutedEvent field directly.
				reExpr = ownerName.IndexOf(':') > 0
					? $"{fqOwner}.{evName}Event"
					: $"System.Windows.EventManager.GetRoutedEvent(typeof({fqOwner}), \"{EscapeString(evName)}\")";
			}
			w.Line($"var {trigVar} = new System.Windows.EventTrigger {{ RoutedEvent = {reExpr} }};");

			// Actions inside <EventTrigger>.
			foreach (XElement actionEl in el.Elements())
			{
				if (actionEl.Name.LocalName != "BeginStoryboard") continue;

				string bsVar = $"__bs{counter++}";
				w.Line($"var {bsVar} = new System.Windows.BeginStoryboard();");
				EmitTimelineName(w, actionEl, xNs, bsVar);

				// Inline <Storyboard> child of <BeginStoryboard>.
				foreach (XElement sbEl in actionEl.Elements())
				{
					if (sbEl.Name.LocalName != "Storyboard") continue;
					string sbVar = EmitStoryboardInline(w, sbEl, xNs, ref counter);
					w.Line($"{bsVar}.Storyboard = {sbVar};");
					break;
				}

				w.Line($"{trigVar}.Actions.Add({bsVar});");
			}

			w.Line($"{containerVar}.Triggers.Add({trigVar});");
		}
	}
}
