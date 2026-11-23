using System.Windows.Data;

namespace System.Windows;

public partial class NameScope
{
	static void OnNameScopeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		BindingEngine.NameScopeChanged(null);

	// Bindings by ElementName re-resolve when a name appears or goes.
	static void NameChanged(string name) => BindingEngine.NameScopeChanged(name);
}
