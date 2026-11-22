namespace System.Windows;

public sealed partial class PropertyPath
{
	internal string ResolvedString =>
		Path == "(0)" && PathParameters.Count == 1 && PathParameters[0] is DependencyProperty dp ? dp.Name : Path;
}
