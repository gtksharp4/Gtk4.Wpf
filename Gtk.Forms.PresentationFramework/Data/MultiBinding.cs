using System.Collections.Generic;

namespace System.Windows.Data
{
	/// <summary>
	/// Combines multiple child <see cref="Binding"/>s into a single target value via
	/// an <see cref="IMultiValueConverter"/>.
	/// <para>
	/// Each child binding resolves independently; the converter receives an array of their
	/// current values and returns the value to push to the target. Source updates are not
	/// supported in this phase (the converter's <c>ConvertBack</c> is not called).
	/// </para>
	/// </summary>
	public class MultiBinding : BindingBase
	{
		/// <summary>Child bindings whose values are passed to <see cref="Converter"/>.</summary>
		public IList<Binding> Bindings { get; } = new List<Binding>();

		/// <summary>Converts the array of child-binding values to the target value.</summary>
		public IMultiValueConverter? Converter { get; set; }

		/// <summary>Optional parameter passed to <see cref="Converter"/>.</summary>
		public object? ConverterParameter { get; set; }

		/// <summary>Direction of data flow. Default is <see cref="BindingMode.OneWay"/>.</summary>
		public BindingMode Mode { get; set; } = BindingMode.Default;
	}
}
