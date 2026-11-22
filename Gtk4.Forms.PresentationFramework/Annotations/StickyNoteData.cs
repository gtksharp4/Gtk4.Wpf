using System.Text;
using System.Xml;

namespace System.Windows.Annotations;

/// <summary>
/// The cargo layout WPF uses for sticky notes: "Meta Data" holds the position attributes and "Text Data" (or
/// "Ink Data") the content; text is a base64 XAML Section, which is what the AnnotatedDocumentViewer sample decodes.
/// </summary>
static class StickyNoteData
{
	internal const string MetaName = "Meta Data";
	internal const string TextName = "Text Data";
	internal const string InkName = "Ink Data";

	internal static readonly XmlQualifiedName TextType =
		new XmlQualifiedName("TextStickyNote", AnnotationXmlConstants.BaseNamespace);

	internal static readonly XmlQualifiedName InkType =
		new XmlQualifiedName("InkStickyNote", AnnotationXmlConstants.BaseNamespace);

	const string Section = "<Section xml:space=\"preserve\" xmlns=\"" + AnnotationXmlConstants.PresentationNamespace
		+ "\"><Paragraph>{0}</Paragraph></Section>";

	internal static void AddCargos(Annotation annotation)
	{
		var doc = new XmlDocument();
		var meta = new AnnotationResource(MetaName);
		var element = doc.CreateElement(AnnotationXmlConstants.BasePrefix, "MetaData",
			AnnotationXmlConstants.BaseNamespace);
		element.SetAttribute("ZOrder", AnnotationXmlConstants.BaseNamespace, "0");
		meta.Contents.Add(element);
		annotation.Cargos.Add(meta);
		bool text = annotation.AnnotationType.Equals(TextType);
		var data = new AnnotationResource(text ? TextName : InkName);
		var content = doc.CreateElement(AnnotationXmlConstants.BasePrefix, text ? "Text" : "Ink",
			AnnotationXmlConstants.BaseNamespace);
		if (text) content.InnerText = Encode(string.Empty);
		data.Contents.Add(content);
		annotation.Cargos.Add(data);
	}

	internal static string Encode(string text) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(
			string.Format(Section, new XmlDocument().CreateTextNode(text).OuterXml)));

	/// <summary>The plain text of a text note, or empty when the cargo is missing or unreadable.</summary>
	internal static string ReadText(Annotation annotation)
	{
		var content = Cargo(annotation, TextName)?.Contents;
		if (content == null || content.Count == 0) return string.Empty;
		try
		{
			var xml = Encoding.UTF8.GetString(Convert.FromBase64String(content[0].InnerText));
			var doc = new XmlDocument();
			doc.LoadXml(xml);
			return doc.DocumentElement!.InnerText;
		}
		catch (Exception e) when (e is FormatException || e is XmlException)
		{
			return string.Empty;
		}
	}

	/// <summary>Replaces the text cargo's content, so the store sees a cargo change.</summary>
	internal static void WriteText(Annotation annotation, string text)
	{
		var cargo = Cargo(annotation, TextName);
		if (cargo == null || cargo.Contents.Count == 0) return;
		var content = cargo.Contents[0].OwnerDocument.CreateElement(AnnotationXmlConstants.BasePrefix, "Text",
			AnnotationXmlConstants.BaseNamespace);
		content.InnerText = Encode(text);
		cargo.Contents[0] = content;
	}

	internal static AnnotationResource? Cargo(Annotation annotation, string name)
	{
		foreach (var cargo in annotation.Cargos)
			if (cargo.Name == name) return cargo;
		return null;
	}
}
