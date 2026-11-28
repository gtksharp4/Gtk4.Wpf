namespace System.Windows.Data;

public partial class Binding
{
	/// <summary>
	/// Gets or sets an XPath query evaluated against an <see cref="System.Xml.XmlNode"/> source (the DataContext,
	/// <see cref="Source"/>, or the Data of a <see cref="DataSourceProvider"/>). <see cref="Path"/>, when set, is applied
	/// to the XPath result. Change notification of the XML document is not implemented.
	/// </summary>
	public string? XPath { get; set; }
}
