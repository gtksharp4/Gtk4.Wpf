using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Windows.Annotations.Storage;

public sealed partial class XmlStreamStore
{
	/// <inheritdoc/>
	public override void Flush()
	{
		lock (SyncRoot)
		{
			CheckStatus();
			if (!_stream!.CanWrite) throw new UnauthorizedAccessException("The stream cannot be written.");
			if (!_dirty) return;
			_stream.Position = 0;
			_stream.SetLength(0);
			var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), CloseOutput = false };
			using (var writer = XmlWriter.Create(_stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement(AnnotationXmlConstants.CorePrefix, AnnotationXmlConstants.Annotations,
					AnnotationXmlConstants.CoreNamespace);
				writer.WriteAttributeString("xmlns", AnnotationXmlConstants.BasePrefix, null,
					AnnotationXmlConstants.BaseNamespace);
				foreach (var annotation in _annotations)
				{
					writer.WriteStartElement(AnnotationXmlConstants.CorePrefix, AnnotationXmlConstants.Annotation,
						AnnotationXmlConstants.CoreNamespace);
					annotation.WriteXml(writer);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
			_stream.Flush();
			_dirty = false;
		}
	}

	void SetStream(Stream stream, IDictionary<Uri, IList<Uri>>? known)
	{
		try
		{
			lock (SyncRoot)
			{
				_stream = stream;
				Load(known);
			}
		}
		catch
		{
			_stream = null;
			_annotations.Clear();
			throw;
		}
	}

	void Load(IDictionary<Uri, IList<Uri>>? known)
	{
		var supported = CheckKnown(known);
		if (_stream!.Length == 0) return;
		if (!_stream.CanRead) throw new ArgumentException("The stream cannot be read.");
		var document = new XmlDocument { PreserveWhitespace = false };
		document.Load(_stream);
		var root = document.DocumentElement;
		if (root == null || root.LocalName != AnnotationXmlConstants.Annotations
			|| root.NamespaceURI != AnnotationXmlConstants.CoreNamespace)
			throw new XmlException("The stream does not hold an annotation document.");
		Strip(root, supported);
		foreach (var node in root.ChildNodes)
		{
			if (!(node is XmlElement element) || element.NamespaceURI != AnnotationXmlConstants.CoreNamespace) continue;
			if (element.LocalName != AnnotationXmlConstants.Annotation) continue;
			var annotation = new Annotation();
			annotation.ReadFrom(element);
			_annotations.Add(annotation);
			Attach(annotation);
		}
	}

	HashSet<string> CheckKnown(IDictionary<Uri, IList<Uri>>? known)
	{
		var all = new HashSet<string>();
		foreach (var uri in _predefined.Keys) all.Add(uri.ToString());
		if (known == null) return all;
		foreach (var key in known.Keys)
		{
			if (key == null) throw new ArgumentException("A namespace is null.", nameof(known));
			if (!all.Add(key.ToString())) throw new ArgumentException("A namespace is duplicated.", nameof(known));
		}
		foreach (var item in known)
		{
			if (item.Value == null) continue;
			foreach (var compat in item.Value)
			{
				if (compat == null) throw new ArgumentException("A namespace is null.", nameof(known));
				if (!all.Add(compat.ToString()))
					throw new ArgumentException("A compatible namespace is duplicated.", nameof(known));
			}
		}
		return all;
	}

	/// <summary>Removes structural elements in unsupported namespaces and records those namespaces.</summary>
	void Strip(XmlElement element, HashSet<string> supported)
	{
		bool isResource = element.NamespaceURI == AnnotationXmlConstants.CoreNamespace
			&& element.LocalName == AnnotationXmlConstants.Resource;
		foreach (var child in new List<XmlNode>(element.ChildNodes.Cast<XmlNode>()))
		{
			if (!(child is XmlElement childElement)) continue;
			if (childElement.NamespaceURI.Length > 0 && !supported.Contains(childElement.NamespaceURI))
			{
				Ignore(childElement.NamespaceURI);
				// Content of a resource is cargo or anchor data: keep it, as WPF keeps non-ignorable content.
				if (!isResource) element.RemoveChild(childElement);
			}
			else Strip(childElement, supported);
		}
	}

	void Ignore(string ns)
	{
		var uri = new Uri(ns, UriKind.RelativeOrAbsolute);
		if (!_ignored.Contains(uri)) _ignored.Add(uri);
	}
}
