namespace System.Windows.Controls;

public partial class DataTemplate
{
	/// <summary>The key an implicit template is stored under, or null when <see cref="DataType"/> is null.</summary>
	public object? DataTemplateKey => DataType != null ? new System.Windows.DataTemplateKey(DataType) : null;
}
