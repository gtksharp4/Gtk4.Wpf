namespace System.ComponentModel;

/// <summary>
/// Defines the direction and property name to sort by.
/// Mirrors <c>System.ComponentModel.SortDescription</c> from WPF's WindowsBase.
/// Note: <c>ListSortDirection</c> is used from the BCL (System.ComponentModel.TypeConverter).
/// </summary>
public struct SortDescription
{
	string? _propertyName;
	ListSortDirection _direction;
	bool _sealed;

	/// <summary>Initialises a new <see cref="SortDescription"/>.</summary>
	/// <exception cref="InvalidEnumArgumentException"><paramref name="direction"/> is not a defined direction.</exception>
	public SortDescription(string? propertyName, ListSortDirection direction)
	{
		Validate(direction, nameof(direction));
		_propertyName = propertyName;
		_direction = direction;
		_sealed = false;
	}

	/// <summary>The property name used as the sort key. <c>null</c> or empty sorts by the item itself.</summary>
	/// <exception cref="InvalidOperationException">The description belongs to a collection.</exception>
	public string? PropertyName
	{
		get => _propertyName;
		set
		{
			CheckSealed();
			_propertyName = value;
		}
	}

	/// <summary>Direction of the sort.</summary>
	/// <exception cref="InvalidEnumArgumentException">The value is not a defined direction.</exception>
	/// <exception cref="InvalidOperationException">The description belongs to a collection.</exception>
	public ListSortDirection Direction
	{
		get => _direction;
		set
		{
			CheckSealed();
			Validate(value, nameof(value));
			_direction = value;
		}
	}

	/// <summary>True once the description is held by a <see cref="SortDescriptionCollection"/>.</summary>
	public bool IsSealed => _sealed;

	internal void Seal() => _sealed = true;

	void CheckSealed()
	{
		if (_sealed)
			throw new InvalidOperationException("Cannot change a SortDescription that is in a SortDescriptionCollection.");
	}

	static void Validate(ListSortDirection direction, string paramName)
	{
		if (direction != ListSortDirection.Ascending && direction != ListSortDirection.Descending)
			throw new InvalidEnumArgumentException(paramName, (int)direction, typeof(ListSortDirection));
	}

	/// <summary>Compares the property name and direction.</summary>
	public override bool Equals(object? obj) => obj is SortDescription other && this == other;

	/// <summary>Hashes the property name and direction.</summary>
	public override int GetHashCode()
	{
		int name = _propertyName == null ? 0 : _propertyName.GetHashCode();
		return unchecked(name * 31 + (int)_direction);
	}

	/// <summary>Compares two descriptions by property name and direction.</summary>
	public static bool operator ==(SortDescription left, SortDescription right) =>
		left._propertyName == right._propertyName && left._direction == right._direction;

	/// <summary>Compares two descriptions by property name and direction.</summary>
	public static bool operator !=(SortDescription left, SortDescription right) => !(left == right);
}
