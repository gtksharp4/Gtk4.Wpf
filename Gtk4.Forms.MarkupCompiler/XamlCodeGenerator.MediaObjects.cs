using System;
using System.Linq;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string MediaNs = "System.Windows.Media.";
		internal const string PresentationOptionsNs =
			"http://schemas.microsoft.com/winfx/2006/xaml/presentation/options";

		// Brushes, drawings, geometries: plain object-model types built from attributes and property elements.
		static bool IsMediaObjectType(string fq)
			=> fq != null && fq.StartsWith(MediaNs, StringComparison.Ordinal)
				&& !fq.StartsWith(MediaNs + "Animation.", StringComparison.Ordinal);

		/// <summary>The type of a keyed media resource element that is built, else <c>null</c>.</summary>
		string MediaResourceType(XElement item)
		{
			if (item.Name.NamespaceName != WpfXmlnsMap.PresentationNs) return null;
			string fq = ResolveType(item);
			return fq != null && MediaGraphEmittable(item, fq) ? fq : null;
		}

		bool IsPlainAttribute(XAttribute a)
			=> !a.IsNamespaceDeclaration && a.Name.Namespace != _xNs && a.Name.Namespace != XNamespace.Xml
				&& !IsIgnorableAttribute(a);

		// Every attribute must convert safely and every nested element must be assignable; a property or
		// type the layer lacks is skipped on emission.
		bool MediaGraphEmittable(XElement el, string fq)
		{
			if (!IsMediaObjectType(fq) || !_resolver.IsInstantiable(fq)) return false;
			if (el.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			foreach (XAttribute a in el.Attributes().Where(IsPlainAttribute))
			{
				string n = a.Name.LocalName;
				if (SkippableMediaAttribute(el, fq, a)) continue;
				if (n.IndexOf('.') >= 0) return false;
				if (_resolver.GetPropertyType(fq, n) == null) continue;
				if (!_resolver.IsWritable(fq, n) || !ClrAttributeConvertible(fq, n, a.Value)) return false;
			}
			foreach (XElement child in el.Elements())
			{
				if (child.Name.LocalName.IndexOf('.') >= 0)
				{
					if (!MediaPropertyElementEmittable(el, fq, child)) return false;
					continue;
				}
				string cfq = ResolveType(child);
				if (cfq == null) continue;
				ContentInfo ci = GetContentInfo(fq);
				Type item = ci != null && ci.IsCollection ? _resolver.CollectionItemType(fq, ci.Name) : null;
				if (item == null || !_resolver.IsAssignable(cfq, item) || !MediaGraphEmittable(child, cfq))
					return false;
			}
			return true;
		}

		// A Visual-typed property element (VisualBrush.Visual) holds a regular element tree.
		bool IsVisualChild(string fq) => !IsMediaObjectType(fq) && _resolver.DerivesFromByName(fq, MediaNs + "Visual");

		bool MediaPropertyElementEmittable(XElement parent, string fq, XElement pe)
		{
			string local = pe.Name.LocalName;
			int dot = local.IndexOf('.');
			if (local.Substring(0, dot) != parent.Name.LocalName) return false;
			string prop = local.Substring(dot + 1);
			Type pt = _resolver.GetPropertyType(fq, prop);
			if (pt == null) return true;
			if (pe.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value))) return false;
			Type item = _resolver.CollectionItemType(fq, prop);
			int count = 0;
			foreach (XElement child in pe.Elements())
			{
				string cfq = ResolveType(child);
				if (cfq == null) continue;
				if (!_resolver.IsAssignable(cfq, item ?? pt) || !(IsVisualChild(cfq) || MediaGraphEmittable(child, cfq)))
					return false;
				count++;
			}
			return item != null || (count <= 1 && _resolver.IsWritable(fq, prop));
		}

		// A property element of a media object: <DrawingBrush.Drawing>, <DrawingGroup.Children>, ...
		bool TryEmitMediaPropertyElement(
			IndentedWriter w, XElement pe, XNamespace xNs, string parentVar, string prop, ref int counter)
		{
			string fq = pe.Parent == null ? null : ResolveType(pe.Parent);
			if (!_resolver.HasReferences || !IsMediaObjectType(fq) || !MediaPropertyElementEmittable(pe.Parent, fq, pe))
				return false;
			if (_resolver.GetPropertyType(fq, prop) == null)
			{
				NoteSkippedMedia(pe.Parent, "property element " + prop);
				return true;
			}
			bool coll = _resolver.CollectionItemType(fq, prop) != null;
			foreach (XElement child in pe.Elements())
			{
				string cfq = ResolveType(child);
				var valueType = _resolver.CollectionItemType(fq, prop) ?? _resolver.GetPropertyType(fq, prop);
				if (cfq == null && TryEmitResourceElementValue(w, child, parentVar, prop, valueType, coll)) continue;
				if (cfq == null)
				{
					NoteSkippedMedia(pe, "element " + child.Name.LocalName);
					continue;
				}
				string v = $"__e{counter++}";
				EmitElement(w, child, xNs, v, cfq, ref counter);
				w.Line(coll ? $"{parentVar}.{prop}.Add({v});" : $"{parentVar}.{prop} = {v};");
			}
			return true;
		}

		// A media property the layer lacks (TileBrush.Viewbox) is dropped instead of breaking the build.
		bool SkipsUnknownMediaProperty(string fq, string prop)
			=> _resolver.HasReferences && IsMediaObjectType(fq) && prop.IndexOf('.') < 0
				&& _resolver.GetPropertyType(fq, prop) == null && !IsKnownEvent(fq, prop);

		// A {StaticResource}/{DynamicResource} on a settable media reference-type property. DynamicResource is
		// resolved once and tolerates a missing key (null).
		Type MediaResourceTarget(string fq, string name, string val)
		{
			if (!IsMediaObjectType(fq) || !IsStaticOrDynamicResource(val, out string key)) return null;
			// Only plain and {x:Type} keys resolve; others ({x:Static ...}) keep the fallback.
			if (key.StartsWith("{", StringComparison.Ordinal)
				&& !key.StartsWith("{x:Type", StringComparison.Ordinal)) return null;
			Type t = _resolver.GetPropertyType(fq, name);
			return t != null && !t.IsValueType && !t.IsGenericType && t.FullName != null
				&& t.FullName.StartsWith(MediaNs, StringComparison.Ordinal) && _resolver.IsWritable(fq, name)
				? t : null;
		}

		bool ClrAttributeConvertible(string fq, string name, string val)
		{
			if (IsStaticOrDynamicResource(val, out _)) return MediaResourceTarget(fq, name, val) != null;
			if (IsBindingExpression(val, out _)) return IsKnownDp(fq, name);
			if (val.TrimStart().StartsWith("{", StringComparison.Ordinal)) return false;
			Type t = _resolver.GetPropertyType(fq, name);
			if (t == null) return false;
			Type under = Nullable.GetUnderlyingType(t) ?? t;
			bool quoted = ClrPropertyExpression(fq, name, val).StartsWith("\"", StringComparison.Ordinal);
			if (under == typeof(string)) return quoted;
			if (under == typeof(object)) return true;
			if (quoted) return false;
			if (under == typeof(bool) || under == typeof(double) || under.IsEnum || under == typeof(Uri)) return true;
			if (IsIntegral(under) || under == typeof(float) || under == typeof(decimal))
				return long.TryParse(val, System.Globalization.NumberStyles.Integer,
					System.Globalization.CultureInfo.InvariantCulture, out _);
			return ConverterBackedStructs.Contains(under.FullName) || ConverterBackedClasses.Contains(under.FullName)
				|| under.FullName == MediaNs + "Geometry" || under.FullName == MediaNs + "PathFigureCollection";
		}

		static bool IsIntegral(Type t)
			=> t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte)
				|| t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort) || t == typeof(sbyte);
	}
}
