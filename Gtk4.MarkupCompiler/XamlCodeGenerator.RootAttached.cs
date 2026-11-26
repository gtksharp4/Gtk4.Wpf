namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Emits an attached property ("Owner.Prop") set on <paramref name="target"/> through the owner type
		/// resolved from the xmlns mapping, never as a member of the target.
		/// </summary>
		void EmitRootAttached(IndentedWriter w, string target, string propName, string val)
		{
			int dot = propName.IndexOf('.');
			string ownerFqType = QualifyTypeName(propName.Substring(0, dot));
			string attachedProp = propName.Substring(dot + 1);
			string ownerEmit = Emitted(ownerFqType);
			if (IsTemplateBindingExpression(val, out string binding) || IsBindingExpression(val, out binding))
			{
				w.Line($"{target}.SetBinding({ownerEmit}.{attachedProp}Property, {binding});");
				return;
			}

			string castType = ResolveAttachedPropType(ownerFqType, attachedProp);
			w.Line($"{ownerEmit}.Set{attachedProp}({target}, " +
				$"({castType})System.Windows.Markup.XamlValueConverter.Convert(" +
				$"typeof({ownerEmit}), {ownerEmit}.{attachedProp}Property, \"{EscapeString(val)}\"));");
		}
	}
}
