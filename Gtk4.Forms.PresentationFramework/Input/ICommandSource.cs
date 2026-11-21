namespace System.Windows.Input;

/// <summary>
/// Implemented by elements that can invoke an <see cref="ICommand"/>
/// (e.g. <see cref="System.Windows.Controls.Primitives.ButtonBase"/>,
/// <see cref="System.Windows.Controls.MenuItem"/>).
/// </summary>
public interface ICommandSource
{
	/// <summary>The command to invoke.</summary>
	ICommand? Command { get; }

	/// <summary>Parameter passed to <see cref="ICommand.Execute"/>.</summary>
	object? CommandParameter { get; }

	/// <summary>
	/// Optional explicit target for routed command dispatch.
	/// When <c>null</c> the element itself is the target.
	/// </summary>
	System.Windows.IInputElement? CommandTarget { get; }
}