using System;
using System.Xml.Linq;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		const string FrameworkContentElementType = "System.Windows.FrameworkContentElement";

		// FrameworkElement and FrameworkContentElement carry SetBinding; any other DependencyObject (a freezable,
		// a CollectionViewSource, an animation) is bound through BindingOperations.
		bool HasSetBinding(string fqType)
		{
			if (string.IsNullOrEmpty(fqType) || !_resolver.HasReferences) return true;
			string bare = fqType.Replace("global::", "");
			if (!_resolver.TypeExists(bare) || !_resolver.IsDependencyObject(bare)) return true;
			return _resolver.DerivesFromByName(bare, FrameworkElementType)
				|| _resolver.DerivesFromByName(bare, FrameworkContentElementType);
		}

		string ParentFqType(XElement el) => el.Parent == null ? null : ResolveType(el.Parent);

		// The statement binding <paramref name="dp"/> on <paramref name="target"/> (of type fqType) to the binding.
		string SetBindingLine(string target, string fqType, string dp, string binding)
		{
			if (HasSetBinding(fqType)) return $"{target}.SetBinding({dp}, {binding});";
			string prop = dp.Substring(dp.LastIndexOf('.') + 1);
			if (prop.EndsWith("Property", StringComparison.Ordinal)) prop = prop.Substring(0, prop.Length - 8);
			Note($"binding on non-element {fqType?.Replace("global::", "")}.{prop} is not evaluated.");
			return $"global::System.Windows.Markup.NonElementBinding.TrySetBinding({target}, {dp}, {binding});";
		}
	}
}
