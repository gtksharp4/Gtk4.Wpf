using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace System.Windows.Data;

/// <summary>
/// Declares a one-way (Phase-0) or two-way (Phase-1) data binding.
/// Round 7: adds <see cref="RelativeSource"/>, <see cref="UpdateSourceTrigger"/>.
/// Round 9: adds validation properties.
/// <see cref="BindingBase.StringFormat"/>, <see cref="BindingBase.FallbackValue"/> and
/// <see cref="BindingBase.TargetNullValue"/> are inherited from <see cref="BindingBase"/>.
/// </summary>
public partial class Binding : BindingBase
{
	/// <summary>
	/// Sentinel returned from <see cref="IValueConverter.ConvertBack"/> (or
	/// <see cref="IMultiValueConverter.ConvertBack"/>) to indicate that no value should be
	/// written back to the source — equivalent to WPF's <c>Binding.DoNothing</c>.
	/// </summary>
	public static readonly object DoNothing = new DoNothingSentinel();

	sealed class DoNothingSentinel
	{
		public override string ToString() => "{Binding.DoNothing}";
	}


	/// <summary>Creates a binding with an empty path (binds to the source object itself).</summary>
	public Binding() { }

	/// <summary>Creates a binding with the specified property path.</summary>
	public Binding(string path)
	{
		if (path != null) Path = new PropertyPath(path, (object[])null!);
	}

	/// <summary>
	/// Property path on the source object (e.g. <c>"Name"</c>,
	/// <c>"Address.City"</c>, or one <see cref="DependencyProperty"/> via <c>(0)</c>). Null binds to the source itself.
	/// </summary>
	public PropertyPath? Path { get; set; }

	/// <summary>Direction of data flow. Default is <see cref="BindingMode.Default"/>.</summary>
	public BindingMode Mode { get; set; } = BindingMode.Default;

	/// <summary>
	/// Explicit binding source. When null the element's inherited
	/// <c>DataContext</c> is used (unless <see cref="ElementName"/> or
	/// <see cref="RelativeSource"/> is set).
	/// </summary>
	public object? Source { get; set; }

	/// <summary>
	/// Name of another element in the same name scope to use as the source.
	/// </summary>
	public string? ElementName { get; set; }

	/// <summary>
	/// Gets or sets whether the path is evaluated against a <see cref="DataSourceProvider"/> source itself rather
	/// than against its <see cref="DataSourceProvider.Data"/>. Default is false.
	/// </summary>
	public bool BindsDirectlyToSource { get; set; }

	/// <summary>
	/// Describes the source as a position relative to the target element.
	/// Supported modes: <see cref="RelativeSourceMode.Self"/>,
	/// <see cref="RelativeSourceMode.FindAncestor"/> (with <c>AncestorType</c>/
	/// <c>AncestorLevel</c>), <see cref="RelativeSourceMode.TemplatedParent"/>.
	/// </summary>
	public RelativeSource? RelativeSource { get; set; }

	/// <summary>Optional value converter applied between source and target.</summary>
	public IValueConverter? Converter { get; set; }

	/// <summary>Optional parameter passed to <see cref="Converter"/>.</summary>
	public object? ConverterParameter { get; set; }

	/// <summary>
	/// When the source is updated for <see cref="BindingMode.TwoWay"/> or
	/// <see cref="BindingMode.OneWayToSource"/> bindings.
	/// <see cref="UpdateSourceTrigger.PropertyChanged"/>, <see cref="UpdateSourceTrigger.LostFocus"/>
	/// (written when <c>UIElement.LostFocusEvent</c> reaches the target) and
	/// <see cref="UpdateSourceTrigger.Explicit"/> are honoured; Default resolves from the target metadata.
	/// </summary>
	public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.Default;

	// ── Validation ───────────────────────────────────────────────────────────

	/// <summary>Collection of <see cref="ValidationRule"/>s evaluated on source update.</summary>
	public Collection<ValidationRule> ValidationRules { get; } = new Collection<ValidationRule>();

	/// <summary>
	/// When <c>true</c>, checks <see cref="IDataErrorInfo"/> on the source after a TwoWay update
	/// and on each source-to-target transfer.
	/// </summary>
	public bool ValidatesOnDataErrors { get; set; }

	/// <summary>
	/// When <c>true</c> (default), surfaces <see cref="INotifyDataErrorInfo"/> errors of the source
	/// and tracks its <c>ErrorsChanged</c> event.
	/// </summary>
	public bool ValidatesOnNotifyDataErrors { get; set; } = true;

	/// <summary>
	/// Culture passed to converters, validation rules and <c>StringFormat</c>. When <c>null</c> the
	/// target's language is used; <c>FrameworkElement.Language</c> is not implemented so that is
	/// WPF's default, <c>en-US</c>.
	/// </summary>
	public System.Globalization.CultureInfo? ConverterCulture { get; set; }

	/// <summary>
	/// When <c>true</c>, any exception thrown during a source update is caught and surfaced
	/// as a validation error via <see cref="ExceptionValidationRule"/>.
	/// </summary>
	public bool ValidatesOnExceptions { get; set; }

	/// <summary>
	/// When <c>true</c>, validation errors are reported via the
	/// <see cref="Validation.ErrorEvent"/> attached routed event.
	/// </summary>
	public bool NotifyOnValidationError { get; set; }
}