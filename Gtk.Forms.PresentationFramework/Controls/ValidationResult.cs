namespace System.Windows.Controls;

/// <summary>
/// Represents the result returned by a <see cref="ValidationRule"/>.
/// </summary>
public class ValidationResult
{
	/// <summary>Singleton representing a valid result.</summary>
	public static readonly ValidationResult ValidResult = new ValidationResult(true, null);

	/// <summary><c>true</c> when the value is valid.</summary>
	public bool IsValid { get; }

	/// <summary>Error content when <see cref="IsValid"/> is <c>false</c>; otherwise <c>null</c>.</summary>
	public object? ErrorContent { get; }

	/// <summary>Initialises a new <see cref="ValidationResult"/>.</summary>
	public ValidationResult(bool isValid, object? errorContent)
	{
		IsValid = isValid;
		ErrorContent = errorContent;
	}
}