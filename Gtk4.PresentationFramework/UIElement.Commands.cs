using System.Windows.Input;

namespace System.Windows;

public abstract partial class UIElement
{
	/// <summary>
	/// The collection of <see cref="CommandBinding"/> objects attached to this element.
	/// <see cref="CommandManager"/> walks this collection when routing commands.
	/// </summary>
	public CommandBindingCollection CommandBindings { get; } = new CommandBindingCollection();
}