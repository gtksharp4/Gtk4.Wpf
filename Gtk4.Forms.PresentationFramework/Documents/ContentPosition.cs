namespace System.Windows.Documents;

/// <summary>Marks a position inside paginated content; concrete paginators derive their own.</summary>
public abstract class ContentPosition
{
	/// <summary>A position that is not in the content.</summary>
	public static readonly ContentPosition Missing = new MissingContentPosition();

	sealed class MissingContentPosition : ContentPosition
	{
	}
}
