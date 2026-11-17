namespace System.Windows.Controls;

/// <summary>
/// Represents a validation error that is created either by the binding engine when
/// a <see cref="ValidationRule"/> reports an error, or by an explicitly specified error
/// via the <see cref="Validation.MarkInvalid"/> method.
/// </summary>
public class ValidationError
{
	/// <summary>The rule that caused this error, or <c>null</c> for engine-generated errors.</summary>
	public ValidationRule? RuleInError { get; }

	/// <summary>The error content (message string or object).</summary>
	public object? ErrorContent { get; }

	/// <summary>The exception that caused this error, or <c>null</c>.</summary>
	public Exception? Exception { get; }

	/// <summary>Initialises a new <see cref="ValidationError"/>.</summary>
	public ValidationError(ValidationRule? rule, object? errorContent, Exception? exception = null)
	{
		RuleInError = rule;
		ErrorContent = errorContent;
		Exception = exception;
	}
}