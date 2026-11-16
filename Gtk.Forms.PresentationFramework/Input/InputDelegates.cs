namespace System.Windows.Input
{
	/// <summary>Handler delegate for keyboard key events.</summary>
	public delegate void KeyEventHandler(object sender, KeyEventArgs e);

	/// <summary>Handler delegate for mouse move / enter / leave events.</summary>
	public delegate void MouseEventHandler(object sender, MouseEventArgs e);

	/// <summary>Handler delegate for mouse button press / release events.</summary>
	public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);

	/// <summary>Handler delegate for text-composition (text input) events.</summary>
	public delegate void TextCompositionEventHandler(object sender, TextCompositionEventArgs e);
}

namespace System.Windows
{
	/// <summary>
	/// Minimal <c>IInputElement</c> contract — implemented by <see cref="UIElement"/>.
	/// Allows <see cref="Input.Keyboard"/> and <see cref="Input.FocusManager"/> to reference
	/// elements without depending on the concrete class.
	/// </summary>
	public interface IInputElement
	{
		/// <summary>Whether this element can receive focus.</summary>
		bool Focusable { get; set; }

		/// <summary>Requests keyboard focus.</summary>
		bool Focus();
	}
}
