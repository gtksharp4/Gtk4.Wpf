using System;
using System.Windows.Input;

namespace System.Windows
{
	public abstract partial class UIElement
	{
		/// <summary>
		/// The collection of <see cref="InputBinding"/> objects on this element.
		/// Call <see cref="InvokeInputBinding"/> to dispatch a gesture programmatically;
		/// GTK key/mouse event wiring is a later phase.
		/// </summary>
		public InputBindingCollection InputBindings { get; } = new InputBindingCollection();

		/// <summary>
		/// Checks each <see cref="InputBinding"/> in <see cref="InputBindings"/> against
		/// <paramref name="inputEventArgs"/>; for the first matching binding whose command
		/// <see cref="ICommand.CanExecute"/> returns <c>true</c>, executes the command.
		/// </summary>
		/// <param name="inputEventArgs">
		/// The event-args to match (e.g. a <see cref="KeyGestureEventArgs"/> or
		/// <see cref="MouseGestureEventArgs"/>).
		/// </param>
		/// <returns>
		/// <c>true</c> if a binding matched and the command was invoked; otherwise <c>false</c>.
		/// </returns>
		public bool InvokeInputBinding(EventArgs inputEventArgs)
		{
			if (inputEventArgs == null) throw new ArgumentNullException(nameof(inputEventArgs));
			foreach (var binding in InputBindings)
			{
				if (binding.Gesture == null || binding.Command == null) continue;
				if (!binding.Gesture.Matches(this, inputEventArgs)) continue;
				if (!binding.Command.CanExecute(binding.CommandParameter)) continue;
				binding.Command.Execute(binding.CommandParameter);
				return true;
			}
			return false;
		}
	}
}
