namespace System.Windows.Xps.Packaging;

/// <summary>Namespaces, relationship types and content types of the XPS format.</summary>
static class XpsNames
{
	internal const string Markup = "http://schemas.microsoft.com/xps/2005/06";
	internal const string ResourceKey = "http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key";
	internal const string Relationships = "http://schemas.openxmlformats.org/package/2006/relationships";
	internal const string ContentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";
	internal const string FixedRepresentation = "http://schemas.microsoft.com/xps/2005/06/fixedrepresentation";
	internal const string RequiredResource = "http://schemas.microsoft.com/xps/2005/06/required-resource";
	internal const string RelationshipsType = "application/vnd.openxmlformats-package.relationships+xml";
	internal const string SequenceType = "application/vnd.ms-package.xps-fixeddocumentsequence+xml";
	internal const string DocumentType = "application/vnd.ms-package.xps-fixeddocument+xml";
	internal const string PageType = "application/vnd.ms-package.xps-fixedpage+xml";
	internal const string RootRelationships = "/_rels/.rels";
	internal const string ContentTypesEntry = "[Content_Types].xml";
}
