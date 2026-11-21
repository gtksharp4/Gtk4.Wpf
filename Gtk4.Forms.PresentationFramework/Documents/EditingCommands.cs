using System.Windows.Input;

namespace System.Windows.Documents;

/// <summary>
/// Standard WPF rich-text editing commands.
/// Each command targets <see cref="System.Windows.Controls.RichTextBox"/> or any
/// control that handles these commands via <see cref="CommandBinding"/>.
/// </summary>
public static partial class EditingCommands
{
	static Type Owner => typeof(EditingCommands);

	static InputGestureCollection Gestures(params InputGesture[] g)
	{
		var c = new InputGestureCollection();
		foreach (var item in g) c.Add(item);
		return c;
	}

	static KeyGesture K(Key k, ModifierKeys m = ModifierKeys.None) =>
		new KeyGesture(k, m);
	static KeyGesture Ctrl(Key k) => K(k, ModifierKeys.Control);

	// ── Typography ─────────────────────────────────────────────────────────────

	/// <summary>Toggle bold (Ctrl+B).</summary>
	public static RoutedUICommand ToggleBold { get; } =
		new RoutedUICommand("Toggle Bold", nameof(ToggleBold), Owner,
			Gestures(Ctrl(Key.B)));

	/// <summary>Toggle italic (Ctrl+I).</summary>
	public static RoutedUICommand ToggleItalic { get; } =
		new RoutedUICommand("Toggle Italic", nameof(ToggleItalic), Owner,
			Gestures(Ctrl(Key.I)));

	/// <summary>Toggle underline (Ctrl+U).</summary>
	public static RoutedUICommand ToggleUnderline { get; } =
		new RoutedUICommand("Toggle Underline", nameof(ToggleUnderline), Owner,
			Gestures(Ctrl(Key.U)));

	/// <summary>Toggle strikethrough.</summary>
	public static RoutedUICommand ToggleStrikethrough { get; } =
		new RoutedUICommand("Toggle Strikethrough", nameof(ToggleStrikethrough), Owner);

	/// <summary>Toggle subscript.</summary>
	public static RoutedUICommand ToggleSubscript { get; } =
		new RoutedUICommand("Toggle Subscript", nameof(ToggleSubscript), Owner,
			Gestures(K(Key.OemPlus, ModifierKeys.Control)));

	/// <summary>Toggle superscript.</summary>
	public static RoutedUICommand ToggleSuperscript { get; } =
		new RoutedUICommand("Toggle Superscript", nameof(ToggleSuperscript), Owner,
			Gestures(K(Key.OemPlus, ModifierKeys.Control | ModifierKeys.Shift)));

	/// <summary>Increase font size.</summary>
	public static RoutedUICommand IncreaseFontSize { get; } =
		new RoutedUICommand("Increase Font Size", nameof(IncreaseFontSize), Owner,
			Gestures(K(Key.OemCloseBrackets, ModifierKeys.Control)));

	/// <summary>Decrease font size.</summary>
	public static RoutedUICommand DecreaseFontSize { get; } =
		new RoutedUICommand("Decrease Font Size", nameof(DecreaseFontSize), Owner,
			Gestures(K(Key.OemOpenBrackets, ModifierKeys.Control)));

	// ── Alignment ─────────────────────────────────────────────────────────────

	/// <summary>Align left (Ctrl+L).</summary>
	public static RoutedUICommand AlignLeft { get; } =
		new RoutedUICommand("Align Left", nameof(AlignLeft), Owner,
			Gestures(Ctrl(Key.L)));

	/// <summary>Align right (Ctrl+R).</summary>
	public static RoutedUICommand AlignRight { get; } =
		new RoutedUICommand("Align Right", nameof(AlignRight), Owner,
			Gestures(Ctrl(Key.R)));

	/// <summary>Align center (Ctrl+E).</summary>
	public static RoutedUICommand AlignCenter { get; } =
		new RoutedUICommand("Align Center", nameof(AlignCenter), Owner,
			Gestures(Ctrl(Key.E)));

	/// <summary>Justify (Ctrl+J).</summary>
	public static RoutedUICommand AlignJustify { get; } =
		new RoutedUICommand("Align Justify", nameof(AlignJustify), Owner,
			Gestures(Ctrl(Key.J)));

	// ── Lists ─────────────────────────────────────────────────────────────────

	/// <summary>Toggle bulleted list.</summary>
	public static RoutedUICommand ToggleBullets { get; } =
		new RoutedUICommand("Toggle Bullets", nameof(ToggleBullets), Owner);

	/// <summary>Toggle numbered list.</summary>
	public static RoutedUICommand ToggleNumbering { get; } =
		new RoutedUICommand("Toggle Numbering", nameof(ToggleNumbering), Owner);

	/// <summary>Increase indentation.</summary>
	public static RoutedUICommand IncreaseIndentation { get; } =
		new RoutedUICommand("Increase Indentation", nameof(IncreaseIndentation), Owner,
			Gestures(K(Key.Tab)));

	/// <summary>Decrease indentation.</summary>
	public static RoutedUICommand DecreaseIndentation { get; } =
		new RoutedUICommand("Decrease Indentation", nameof(DecreaseIndentation), Owner,
			Gestures(K(Key.Tab, ModifierKeys.Shift)));

	// ── Deletion ──────────────────────────────────────────────────────────────

	/// <summary>Delete character forward (Delete).</summary>
	public static RoutedUICommand Delete { get; } =
		new RoutedUICommand("Delete", nameof(Delete), Owner,
			Gestures(K(Key.Delete)));

	/// <summary>Delete word forward (Ctrl+Delete).</summary>
	public static RoutedUICommand DeleteNextWord { get; } =
		new RoutedUICommand("Delete Next Word", nameof(DeleteNextWord), Owner,
			Gestures(Ctrl(Key.Delete)));

	/// <summary>Delete character backward (Backspace).</summary>
	public static RoutedUICommand Backspace { get; } =
		new RoutedUICommand("Backspace", nameof(Backspace), Owner,
			Gestures(K(Key.Back)));

	/// <summary>Delete word backward (Ctrl+Backspace).</summary>
	public static RoutedUICommand DeletePreviousWord { get; } =
		new RoutedUICommand("Delete Previous Word", nameof(DeletePreviousWord), Owner,
			Gestures(Ctrl(Key.Back)));

	// ── Insert ────────────────────────────────────────────────────────────────

	/// <summary>Insert a tab (Tab).</summary>
	public static RoutedUICommand TabForward { get; } =
		new RoutedUICommand("Tab Forward", nameof(TabForward), Owner,
			Gestures(K(Key.Tab)));

	/// <summary>Insert a backward tab (Shift+Tab).</summary>
	public static RoutedUICommand TabBackward { get; } =
		new RoutedUICommand("Tab Backward", nameof(TabBackward), Owner,
			Gestures(K(Key.Tab, ModifierKeys.Shift)));

	/// <summary>Insert a paragraph break (Enter).</summary>
	public static RoutedUICommand EnterParagraphBreak { get; } =
		new RoutedUICommand("Enter Paragraph Break", nameof(EnterParagraphBreak), Owner,
			Gestures(K(Key.Return)));

	/// <summary>Insert a line break (Shift+Enter).</summary>
	public static RoutedUICommand EnterLineBreak { get; } =
		new RoutedUICommand("Enter Line Break", nameof(EnterLineBreak), Owner,
			Gestures(K(Key.Return, ModifierKeys.Shift)));

	// ── Navigation/selection ──────────────────────────────────────────────────

	/// <summary>Move to previous word (Ctrl+Left).</summary>
	public static RoutedUICommand MoveLeftByWord { get; } =
		new RoutedUICommand("Move Left By Word", nameof(MoveLeftByWord), Owner,
			Gestures(Ctrl(Key.Left)));

	/// <summary>Move to next word (Ctrl+Right).</summary>
	public static RoutedUICommand MoveRightByWord { get; } =
		new RoutedUICommand("Move Right By Word", nameof(MoveRightByWord), Owner,
			Gestures(Ctrl(Key.Right)));

	/// <summary>Extend selection to previous word (Ctrl+Shift+Left).</summary>
	public static RoutedUICommand SelectLeftByWord { get; } =
		new RoutedUICommand("Select Left By Word", nameof(SelectLeftByWord), Owner,
			Gestures(K(Key.Left, ModifierKeys.Control | ModifierKeys.Shift)));

	/// <summary>Extend selection to next word (Ctrl+Shift+Right).</summary>
	public static RoutedUICommand SelectRightByWord { get; } =
		new RoutedUICommand("Select Right By Word", nameof(SelectRightByWord), Owner,
			Gestures(K(Key.Right, ModifierKeys.Control | ModifierKeys.Shift)));
}
