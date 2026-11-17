namespace System.Windows;

/// <summary>
/// Provides a format-independent mechanism for transferring data.
/// Mirrors the WPF <c>System.Windows.IDataObject</c> contract.
/// </summary>
public interface IDataObject
{
	/// <summary>Retrieves the data associated with the specified format name.</summary>
	object? GetData(string format);

	/// <summary>Retrieves the data associated with the specified type.</summary>
	object? GetData(Type format);

	/// <summary>
	/// Retrieves the data associated with the specified format, optionally converting
	/// to that format when <paramref name="autoConvert"/> is <see langword="true"/>.
	/// </summary>
	object? GetData(string format, bool autoConvert);

	/// <summary>
	/// Determines whether data stored in this object is associated with
	/// the specified format.
	/// </summary>
	bool GetDataPresent(string format);

	/// <summary>
	/// Determines whether data stored in this object is associated with
	/// the specified type.
	/// </summary>
	bool GetDataPresent(Type format);

	/// <summary>
	/// Determines whether data stored in this object is associated with
	/// the specified format, optionally auto-converting.
	/// </summary>
	bool GetDataPresent(string format, bool autoConvert);

	/// <summary>Returns a list of all formats that data in this object is associated with.</summary>
	string[] GetFormats();

	/// <summary>
	/// Returns a list of all formats that data in this object is associated with,
	/// optionally including auto-convert formats.
	/// </summary>
	string[] GetFormats(bool autoConvert);

	/// <summary>Stores data in this object using the data's type as the format.</summary>
	void SetData(object data);

	/// <summary>Stores data in this object with the specified format.</summary>
	void SetData(string format, object data);

	/// <summary>Stores data in this object using a <see cref="Type"/> as the format.</summary>
	void SetData(Type format, object data);

	/// <summary>
	/// Stores data in this object with the specified format, and optionally
	/// allows the same data to be retrieved in a different format.
	/// </summary>
	void SetData(string format, object data, bool autoConvert);
}
