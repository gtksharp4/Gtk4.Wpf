using System.Globalization;
using System.Reflection;

namespace System.Windows.Data;

/// <summary>The exception thrown when a proposed value is not available.</summary>
public class ValueUnavailableException : Exception
{
	/// <summary>Creates the exception.</summary>
	public ValueUnavailableException()
	{
	}

	/// <summary>Creates the exception with a message.</summary>
	public ValueUnavailableException(string message) : base(message)
	{
	}

	/// <summary>Creates the exception with a message and the exception that caused it.</summary>
	public ValueUnavailableException(string message, Exception innerException) : base(message, innerException)
	{
	}
}

public partial class BindingGroup
{
	/// <summary>Gets the value <paramref name="propertyName"/> of <paramref name="item"/> would have after a commit.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	/// <exception cref="ValueUnavailableException">
	/// No expression of the group binds that property, or its value cannot convert.
	/// </exception>
	public object? GetValue(object item, string propertyName)
	{
		if (!TryGetValue(item, propertyName, out var value))
			throw new ValueUnavailableException(
				"The proposed value of '" + propertyName + "' is not available in this binding group.");
		return value;
	}

	/// <summary>Tries to get the value <paramref name="propertyName"/> of <paramref name="item"/> would have after a commit.</summary>
	/// <returns><see langword="false"/> when no expression binds the property or its value cannot convert.</returns>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	public bool TryGetValue(object item, string propertyName, out object? value)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
		value = null;
		foreach (var expression in BindingExpressions)
		{
			if (!ReferenceEquals(expression.ResolvedSource, item)
			    || PropertyPath.ResolveOwner(item, expression.ParentBinding.PathString) != propertyName)
				continue;
			return TryProposedValue(expression, item, propertyName, out value);
		}
		return false;
	}

	// The target's value, converted back and then to the source property type, as a commit would write it.
	static bool TryProposedValue(BindingExpression expression, object item, string propertyName, out object? value)
	{
		value = null;
		var target = expression.TargetElement;
		if (target == null)
			return false;
		var proposed = target.GetValue(expression.TargetDependencyProperty);
		var property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
		var type = property?.PropertyType ?? typeof(object);
		var binding = expression.ParentBinding;
		var culture = BindingEngine.ResolveCulture(binding.ConverterCulture);
		try
		{
			if (binding.Converter != null)
				proposed = binding.Converter.ConvertBack(proposed, type, binding.ConverterParameter, culture);
		}
		catch (Exception ex) when (!BindingEngine.IsCritical(ex))
		{
			return false;
		}
		if (!BindingEngine.TryConvertToSourceType(proposed, type, out value, out _, culture))
			return false;
		return true;
	}
}
