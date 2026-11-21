using System;

namespace System.Windows.Markup;

/// <summary>Names the property of a type that holds its XAML <c>x:Name</c>.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RuntimeNamePropertyAttribute : Attribute
{
	/// <summary>Creates the attribute with the name of the property that stores the element name.</summary>
	public RuntimeNamePropertyAttribute(string name) => Name = name;

	/// <summary>Gets the name of the property.</summary>
	public string Name { get; }
}
