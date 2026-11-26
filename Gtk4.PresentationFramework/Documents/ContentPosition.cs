namespace System.Windows.Documents;

/// <summary>A position in content, independent of the content type. <see cref="TextPointer"/> is one.</summary>
public abstract class ContentPosition
{
	sealed class MissingPosition : ContentPosition
	{
	}

	/// <summary>Protected so only content-specific positions derive from it.</summary>
	protected ContentPosition()
	{
	}

	/// <summary>The position that stands for "no position".</summary>
	public static readonly ContentPosition Missing = new MissingPosition();
}
