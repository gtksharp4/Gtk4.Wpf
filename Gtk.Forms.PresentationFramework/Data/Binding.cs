namespace System.Windows.Data
{
	/// <summary>
	/// Declares a one-way (Phase-0) or two-way (Phase-1) data binding.
	/// Round 7: adds <see cref="RelativeSource"/>, <see cref="UpdateSourceTrigger"/>.
	/// <see cref="BindingBase.StringFormat"/>, <see cref="BindingBase.FallbackValue"/> and
	/// <see cref="BindingBase.TargetNullValue"/> are inherited from <see cref="BindingBase"/>.
	/// </summary>
	public class Binding : BindingBase
	{
		/// <summary>Creates a binding with an empty path (binds to the source object itself).</summary>
		public Binding() { }

		/// <summary>Creates a binding with the specified property path.</summary>
		public Binding(string path) => Path = path;

		/// <summary>
		/// Dot-separated property path on the source object (e.g. <c>"Name"</c>,
		/// <c>"Address.City"</c>). An empty or null path binds to the source itself.
		/// </summary>
		public string? Path { get; set; }

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
		/// <see cref="UpdateSourceTrigger.PropertyChanged"/> and
		/// <see cref="UpdateSourceTrigger.Explicit"/> are honoured;
		/// <see cref="UpdateSourceTrigger.LostFocus"/> falls back to PropertyChanged.
		/// </summary>
		public UpdateSourceTrigger UpdateSourceTrigger { get; set; } = UpdateSourceTrigger.Default;
	}
}
