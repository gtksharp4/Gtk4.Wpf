namespace System.Windows.Media.Animation;

/// <summary>How a new animation interacts with the animations already on a property.</summary>
public enum HandoffBehavior
{
	/// <summary>Start from the current animated value and replace the existing animations.</summary>
	SnapshotAndReplace = 0,
	/// <summary>Layer the new animation on top of the existing ones.</summary>
	Compose = 1
}
