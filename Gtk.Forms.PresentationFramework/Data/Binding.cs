namespace System.Windows.Data
{
	/// <summary>
	/// Declares a one-way (Phase-0) or two-way (Phase-1) data binding.
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
		/// <c>DataContext</c> is used.
		/// </summary>
		public object? Source { get; set; }

		/// <summary>
		/// Name of another element in the same name scope to use as the source.
		/// Phase-1 feature; set here so the type is complete.
		/// </summary>
		public string? ElementName { get; set; }

		/// <summary>Optional value converter applied between source and target.</summary>
		public IValueConverter? Converter { get; set; }

		/// <summary>Optional parameter passed to <see cref="Converter"/>.</summary>
		public object? ConverterParameter { get; set; }
	}
}
