namespace System.Windows.Controls;

public abstract partial class Control
{
	internal override FrameworkTemplate? TemplateInternal => Template;

	internal override UIElement? TemplateRootInternal => _templateRoot;
}
