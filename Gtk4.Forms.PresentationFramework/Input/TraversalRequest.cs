using System.ComponentModel;

namespace System.Windows.Input;

/// <summary>A request to move focus in a <see cref="FocusNavigationDirection"/>.</summary>
public class TraversalRequest
{
	/// <param name="focusNavigationDirection">The direction to move focus.</param>
	/// <exception cref="InvalidEnumArgumentException">The direction is not a defined value.</exception>
	public TraversalRequest(FocusNavigationDirection focusNavigationDirection)
	{
		if (focusNavigationDirection < FocusNavigationDirection.Next
			|| focusNavigationDirection > FocusNavigationDirection.Down)
			throw new InvalidEnumArgumentException(nameof(focusNavigationDirection),
				(int)focusNavigationDirection, typeof(FocusNavigationDirection));
		FocusNavigationDirection = focusNavigationDirection;
	}

	/// <summary>Whether focus wraps from the last element to the first (and back). Default false.</summary>
	public bool Wrapped { get; set; }

	/// <summary>The direction of the request.</summary>
	public FocusNavigationDirection FocusNavigationDirection { get; }
}
