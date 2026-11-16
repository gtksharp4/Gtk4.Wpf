namespace System.Windows.Controls
{
	/// <summary>
	/// Partial extension of <see cref="SelectionChangedEventArgs"/> that adds a
	/// source-as-object constructor used by <see cref="ListBox"/> and
	/// <see cref="ListView"/>.
	/// <para>
	/// When constructed via this overload <see cref="Source"/> will be <c>null</c>;
	/// use the event sender argument instead.
	/// </para>
	/// </summary>
	public sealed partial class SelectionChangedEventArgs
	{
		/// <summary>
		/// Creates event args for any selection-supporting control (e.g. <see cref="ListBox"/>).
		/// </summary>
		internal SelectionChangedEventArgs(object? source, object? oldValue, object? newValue)
		{
			Source = (source as ComboBox)!;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
