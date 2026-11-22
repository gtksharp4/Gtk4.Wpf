using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace System.Windows.Annotations;

/// <summary>One step of a <see cref="ContentLocator"/>: a typed set of name/value pairs.</summary>
public sealed partial class ContentLocatorPart : INotifyPropertyChanged, IOwned
{
	readonly XmlQualifiedName _type;
	readonly NotifyingDictionary _pairs = new NotifyingDictionary();
	PropertyChangedEventHandler? _changed;
	bool _owned;

	/// <summary>Creates a part of the given type.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="partType"/> is null.</exception>
	/// <exception cref="ArgumentException">The type has no name or no namespace.</exception>
	public ContentLocatorPart(XmlQualifiedName partType)
	{
		if (partType == null) throw new ArgumentNullException(nameof(partType));
		if (string.IsNullOrEmpty(partType.Name)) throw new ArgumentException("A type name is required.", "partType.Name");
		if (string.IsNullOrEmpty(partType.Namespace))
			throw new ArgumentException("A type namespace is required.", "partType.Namespace");
		_type = partType;
		_pairs.Changed += () => _changed?.Invoke(this, new PropertyChangedEventArgs(nameof(NameValuePairs)));
	}

	/// <summary>The name/value pairs that describe the content.</summary>
	public IDictionary<string, string> NameValuePairs => _pairs;

	/// <summary>The part's type.</summary>
	public XmlQualifiedName PartType => _type;

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

	/// <summary>Creates a copy with the same type and pairs.</summary>
	public object Clone()
	{
		var copy = new ContentLocatorPart(_type);
		foreach (var pair in _pairs) copy._pairs.Add(pair.Key, pair.Value);
		return copy;
	}

	/// <summary>True when the other part has the same type and the same pairs.</summary>
	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(obj, this)) return true;
		if (!(obj is ContentLocatorPart other) || !_type.Equals(other._type) || other._pairs.Count != _pairs.Count)
			return false;
		foreach (var pair in _pairs)
			if (!other._pairs.TryGetValue(pair.Key, out var value) || value != pair.Value) return false;
		return true;
	}

	/// <summary>Reference hash, as in WPF (the part is mutable, and WPF does not override it either).</summary>
	public override int GetHashCode() => base.GetHashCode();
}
