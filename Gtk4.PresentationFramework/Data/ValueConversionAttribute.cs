namespace System.Windows.Data;

/// <summary>
/// Identifies the source and target types an <see cref="IValueConverter"/> converts between.
/// Metadata only: the binding engine does not read it.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ValueConversionAttribute : Attribute
{
	/// <summary>Initializes a new instance with the converter's source and target types.</summary>
	/// <exception cref="ArgumentNullException">Either type is <see langword="null"/>.</exception>
	public ValueConversionAttribute(Type sourceType, Type targetType)
	{
		SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
		TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
	}

	/// <summary>The type the converter's <c>Convert</c> receives.</summary>
	public Type SourceType { get; }

	/// <summary>The type the converter's <c>Convert</c> returns.</summary>
	public Type TargetType { get; }

	/// <summary>The type of the converter parameter, if any.</summary>
	public Type? ParameterType { get; set; }

	/// <inheritdoc/>
	public override object TypeId => this;
}
