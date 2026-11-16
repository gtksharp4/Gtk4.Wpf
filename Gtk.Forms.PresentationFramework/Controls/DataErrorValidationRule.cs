using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Controls
{
	/// <summary>
	/// Represents a rule that checks for errors raised by the <see cref="IDataErrorInfo"/> interface.
	/// </summary>
	public sealed class DataErrorValidationRule : ValidationRule
	{
		/// <summary>Singleton instance.</summary>
		public static readonly DataErrorValidationRule Instance = new DataErrorValidationRule();

		/// <summary>Validates against <see cref="IDataErrorInfo"/> if the value implements it.</summary>
		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		{
			// DataErrorInfo checking happens on the source, not the target value.
			// The binding engine invokes this rule with the source object when ValidatesOnDataErrors is set.
			if (value is IDataErrorInfo dataError)
			{
				var msg = dataError.Error;
				if (!string.IsNullOrEmpty(msg))
					return new ValidationResult(false, msg);
			}
			return ValidationResult.ValidResult;
		}
	}
}
