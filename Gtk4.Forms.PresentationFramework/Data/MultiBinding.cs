using System.Collections.Generic;

namespace System.Windows.Data;

/// <summary>
/// Combines multiple child <see cref="Binding"/>s into a single target value via
/// an <see cref="IMultiValueConverter"/>.
/// <para>
/// Each child binding resolves independently; the converter receives an array of their
/// current values and returns the value to push to the target. When the binding is TwoWay,
/// the converter's <c>ConvertBack</c> splits the target value back across the child sources.
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

	/// <summary>
	/// Direction of data flow. Default resolves from the target property metadata
	/// (<c>BindsTwoWayByDefault</c>).
	/// </summary>
	public BindingMode Mode { get; set; } = BindingMode.Default;

	/// <summary>
	/// When the target value is written back. Default resolves from the target property metadata
	/// (LostFocus for <c>TextBox.Text</c>, otherwise PropertyChanged).
	/// </summary>
	public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.Default;

	/// <summary>
	/// Culture passed to the converter and <c>StringFormat</c>. When <c>null</c> the target's language
	/// is used; <c>FrameworkElement.Language</c> is not implemented so that is WPF's default, <c>en-US</c>.
	/// </summary>
	public System.Globalization.CultureInfo? ConverterCulture { get; set; }

	/// <summary>
	/// Default <c>true</c> as in WPF. Store-only: <see cref="MultiBinding"/> has no validation
	/// pipeline here, so INotifyDataErrorInfo of child sources is not surfaced.
	/// </summary>
	public bool ValidatesOnNotifyDataErrors { get; set; } = true;
}
