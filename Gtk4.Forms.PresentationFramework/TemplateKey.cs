using System;
using System.Reflection;

namespace System.Windows;

/// <summary>Base class for the keys of implicitly applied templates. Mirrors <c>TemplateKey</c>.</summary>
public abstract class TemplateKey : ResourceKey
{
	/// <summary>The kind of template a key identifies.</summary>
	protected enum TemplateType
	{
		/// <summary>A table template.</summary>
		TableTemplate,

		/// <summary>A data template.</summary>
		DataTemplate,
	}

	readonly TemplateType _templateType;
	object? _dataType;

	/// <summary>Creates a key of the given kind.</summary>
	protected TemplateKey(TemplateType templateType) => _templateType = templateType;

	/// <summary>Creates a key of the given kind for <paramref name="dataType"/>.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="dataType"/> is null.</exception>
	protected TemplateKey(TemplateType templateType, object dataType)
	{
		_templateType = templateType;
		DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
	}

	/// <summary>The data type (a <see cref="Type"/> or an XML element name) the template applies to.</summary>
	/// <exception cref="ArgumentNullException">The value is null.</exception>
	public object? DataType
	{
		get => _dataType;
		set => _dataType = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <inheritdoc/>
	public override Assembly Assembly => (_dataType as Type)?.Assembly!;

	/// <inheritdoc/>
	public override bool Equals(object? o) =>
		o is TemplateKey other && other._templateType == _templateType && Equals(other._dataType, _dataType);

	/// <inheritdoc/>
	public override int GetHashCode() => (int)_templateType * 397 ^ (_dataType?.GetHashCode() ?? 0);
}

/// <summary>The resource key of an implicit <see cref="Controls.DataTemplate"/>.</summary>
public sealed class DataTemplateKey : TemplateKey
{
	/// <summary>Creates a key with no data type.</summary>
	public DataTemplateKey() : base(TemplateType.DataTemplate) { }

	/// <summary>Creates a key for <paramref name="dataType"/>.</summary>
	public DataTemplateKey(object dataType) : base(TemplateType.DataTemplate, dataType) { }
}
