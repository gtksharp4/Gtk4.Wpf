using System.Xml;

namespace System.Windows.Annotations;

/// <summary>Names and namespace URIs of WPF's annotation XML (core and base schema, 2003/11).</summary>
static class AnnotationXmlConstants
{
	internal const string CoreNamespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core";
	internal const string BaseNamespace = "http://schemas.microsoft.com/windows/annotations/2003/11/base";
	internal const string PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	internal const string CorePrefix = "anc";
	internal const string BasePrefix = "anb";

	internal const string Annotations = "Annotations";
	internal const string Annotation = "Annotation";
	internal const string Resource = "Resource";
	internal const string ContentLocator = "ContentLocator";
	internal const string ContentLocatorGroup = "ContentLocatorGroup";
	internal const string Authors = "Authors";
	internal const string Anchors = "Anchors";
	internal const string Cargos = "Cargos";
	internal const string Item = "Item";
	internal const string StringAuthor = "StringAuthor";

	internal const string Id = "Id";
	internal const string CreationTime = "CreationTime";
	internal const string LastModificationTime = "LastModificationTime";
	internal const string TypeName = "Type";
	internal const string ResourceName = "Name";
	internal const string ItemName = "Name";
	internal const string ItemValue = "Value";

	internal const string SegmentAttribute = "Segment";
	internal const string CountAttribute = "Count";
	internal const string IncludeOverlaps = "IncludeOverlaps";
	internal const char Separator = ',';

	internal static readonly XmlQualifiedName CharacterRange = new XmlQualifiedName("CharacterRange", BaseNamespace);
	internal static readonly XmlQualifiedName Highlight = new XmlQualifiedName("Highlight", BaseNamespace);
}
