namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// A DependencyObject that is not an element (SolidColorBrush, a transform) has SetValue but neither
		// FindResource nor SetResourceReference; its resource values resolve against the enclosing element.
		bool IsNonElementDependencyObject(string fqType) =>
			_resolver.TypeExists(fqType) && _resolver.IsDependencyObject(fqType)
			&& !_resolver.DerivesFromByName(fqType, "System.Windows.FrameworkElement");

		// "(T)XamlValueConverter.CoerceResource(typeof(T), value)" for the property's CLR type, or value as is.
		string CoercedDpValue(string fqType, string propName, string value)
		{
			System.Type t = _resolver.GetPropertyType(fqType, propName);
			if (t == null || t.IsGenericType) return value;
			string name = "global::" + t.FullName.Replace('+', '.');
			return $"({name})System.Windows.Markup.XamlValueConverter.CoerceResource(typeof({name}), {value})";
		}

		bool _registerNames;

		// Registers x:Name in the root's name scope so Storyboard.TargetName and FindName reach elements
		// and Freezables alike (an EllipseGeometry inside Image.Clip is named but is not an element).
		void EmitNameRegistration(IndentedWriter w, string varName, string name)
		{
			if (_registerNames)
				w.Line($"this.RegisterName(\"{EscapeString(name)}\", {varName});");
		}
	}
}
