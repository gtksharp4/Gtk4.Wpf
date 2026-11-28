using System.Xml;

namespace System.Windows.Data;

public partial class Binding
{
	/// <summary>
	/// Identifies the attached <c>XmlNamespaceManager</c> property, which inherits down the tree,
	/// used by <see cref="Binding.XPath"/> queries.
	/// </summary>
	public static readonly DependencyProperty XmlNamespaceManagerProperty = DependencyProperty.RegisterAttached(
		"XmlNamespaceManager", typeof(XmlNamespaceManager), typeof(Binding),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Gets the namespace manager for XPath bindings under <paramref name="target"/>.</summary>
	public static XmlNamespaceManager? GetXmlNamespaceManager(DependencyObject target)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		return (XmlNamespaceManager?)target.GetValue(XmlNamespaceManagerProperty);
	}

	/// <summary>Sets the namespace manager for XPath bindings under <paramref name="target"/>.</summary>
	public static void SetXmlNamespaceManager(DependencyObject target, XmlNamespaceManager? value)
	{
		if (target == null) throw new ArgumentNullException(nameof(target));
		target.SetValue(XmlNamespaceManagerProperty, value);
	}
}
