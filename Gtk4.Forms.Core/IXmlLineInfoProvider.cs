using System.Xml;

namespace Gtk4.Forms.Xaml
{
	public interface IXmlLineInfoProvider
	{
		IXmlLineInfo XmlLineInfo { get; }
	}
}