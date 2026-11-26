namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// BasedOn="{StaticResource {x:Type T}}" names the default (theme) style of T. The layer keeps default styles
		// outside the resource dictionaries, and the control's default style applies underneath anyway, so a type
		// key that no dictionary holds means "no base style" instead of a missing resource.
		string BasedOnLookup(string ownerVar, string key)
		{
			string escaped = EscapeString(key);
			if (!key.TrimStart().StartsWith("{x:Type", System.StringComparison.Ordinal))
				return StaticResourceLookup(ownerVar, escaped);
			var scopes = ScopeVarList(null);
			if (ownerVar != "this" && !scopes.Contains(ownerVar)) scopes.Insert(0, ownerVar);
			return scopes.Count == 0
				? $"{LookupRoot}.TryFindResource({ResourceKeyExpression(escaped)})"
				: $"System.Windows.Markup.StaticResourceScope.TryFind({ResourceKeyExpression(escaped)}, {LookupRoot}, "
					+ string.Join(", ", scopes) + ")";
		}
	}
}
