namespace System.Windows.Controls;

/// <summary>A range of one-based page numbers, both ends included.</summary>
public struct PageRange : IEquatable<PageRange>
{
	/// <summary>Creates a range of the single page <paramref name="page"/>.</summary>
	public PageRange(int page) : this(page, page)
	{
	}

	/// <summary>Creates the range from <paramref name="pageFrom"/> to <paramref name="pageTo"/>.</summary>
	public PageRange(int pageFrom, int pageTo)
	{
		PageFrom = pageFrom;
		PageTo = pageTo;
	}

	/// <summary>Gets or sets the first page.</summary>
	public int PageFrom { get; set; }

	/// <summary>Gets or sets the last page.</summary>
	public int PageTo { get; set; }

	/// <summary>Returns whether both ends are equal.</summary>
	public bool Equals(PageRange other) => PageFrom == other.PageFrom && PageTo == other.PageTo;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is PageRange other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => unchecked(PageFrom * 397) ^ PageTo;

	/// <inheritdoc/>
	public override string ToString() => PageFrom == PageTo ? PageFrom.ToString() : $"{PageFrom}-{PageTo}";

	/// <summary>Returns whether the ranges are equal.</summary>
	public static bool operator ==(PageRange left, PageRange right) => left.Equals(right);

	/// <summary>Returns whether the ranges differ.</summary>
	public static bool operator !=(PageRange left, PageRange right) => !left.Equals(right);
}
