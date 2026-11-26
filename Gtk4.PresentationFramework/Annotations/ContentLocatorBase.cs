using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace System.Windows.Annotations;

/// <summary>Base class of <see cref="ContentLocator"/> and <see cref="ContentLocatorGroup"/>.</summary>
public abstract class ContentLocatorBase : INotifyPropertyChanged, IOwned
{
	PropertyChangedEventHandler? _changed;
	bool _owned;

	internal ContentLocatorBase()
	{
	}

	/// <summary>Creates a deep copy.</summary>
	public abstract object Clone();

	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => _changed += value;
		remove => _changed -= value;
	}

	bool IOwned.Owned
	{
		get => _owned;
		set => _owned = value;
	}

	internal void FireChanged(string name) => _changed?.Invoke(this, new PropertyChangedEventArgs(name));

	/// <summary>Writes the locator as its own element (ContentLocator or ContentLocatorGroup).</summary>
	internal abstract void WriteElement(XmlWriter writer);

	/// <summary>The locator, or the locators of a group, that a query is matched against.</summary>
	internal abstract IEnumerable<ContentLocator> Flatten();

	internal static ContentLocatorBase? ReadElement(XmlElement element)
	{
		if (element.NamespaceURI != AnnotationXmlConstants.CoreNamespace) return null;
		if (element.LocalName == AnnotationXmlConstants.ContentLocator)
		{
			var locator = new ContentLocator();
			locator.ReadFrom(element);
			return locator;
		}
		if (element.LocalName == AnnotationXmlConstants.ContentLocatorGroup)
		{
			var group = new ContentLocatorGroup();
			group.ReadFrom(element);
			return group;
		}
		return null;
	}
}
