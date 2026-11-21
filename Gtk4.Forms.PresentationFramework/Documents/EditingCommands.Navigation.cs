using System.Windows.Input;

namespace System.Windows.Documents;

public static partial class EditingCommands
{
	const ModifierKeys CtrlShift = ModifierKeys.Control | ModifierKeys.Shift;

	static RoutedUICommand Cmd(string name, params InputGesture[] gestures) =>
		new RoutedUICommand(name, name, Owner, Gestures(gestures));

	/// <summary>Toggle insert/overwrite mode (Insert).</summary>
	public static RoutedUICommand ToggleInsert { get; } = Cmd(nameof(ToggleInsert), K(Key.Insert));

	/// <summary>Correct a spelling error.</summary>
	public static RoutedUICommand CorrectSpellingError { get; } = Cmd(nameof(CorrectSpellingError));

	/// <summary>Ignore a spelling error.</summary>
	public static RoutedUICommand IgnoreSpellingError { get; } = Cmd(nameof(IgnoreSpellingError));

	/// <summary>Move one character left (Left).</summary>
	public static RoutedUICommand MoveLeftByCharacter { get; } = Cmd(nameof(MoveLeftByCharacter), K(Key.Left));

	/// <summary>Move one character right (Right).</summary>
	public static RoutedUICommand MoveRightByCharacter { get; } = Cmd(nameof(MoveRightByCharacter), K(Key.Right));

	/// <summary>Extend selection one character left (Shift+Left).</summary>
	public static RoutedUICommand SelectLeftByCharacter { get; } =
		Cmd(nameof(SelectLeftByCharacter), K(Key.Left, ModifierKeys.Shift));

	/// <summary>Extend selection one character right (Shift+Right).</summary>
	public static RoutedUICommand SelectRightByCharacter { get; } =
		Cmd(nameof(SelectRightByCharacter), K(Key.Right, ModifierKeys.Shift));

	/// <summary>Move up one line (Up).</summary>
	public static RoutedUICommand MoveUpByLine { get; } = Cmd(nameof(MoveUpByLine), K(Key.Up));

	/// <summary>Move down one line (Down).</summary>
	public static RoutedUICommand MoveDownByLine { get; } = Cmd(nameof(MoveDownByLine), K(Key.Down));

	/// <summary>Move up one page (PageUp).</summary>
	public static RoutedUICommand MoveUpByPage { get; } = Cmd(nameof(MoveUpByPage), K(Key.PageUp));

	/// <summary>Move down one page (PageDown).</summary>
	public static RoutedUICommand MoveDownByPage { get; } = Cmd(nameof(MoveDownByPage), K(Key.PageDown));

	/// <summary>Move up one paragraph (Ctrl+Up).</summary>
	public static RoutedUICommand MoveUpByParagraph { get; } = Cmd(nameof(MoveUpByParagraph), Ctrl(Key.Up));

	/// <summary>Move down one paragraph (Ctrl+Down).</summary>
	public static RoutedUICommand MoveDownByParagraph { get; } = Cmd(nameof(MoveDownByParagraph), Ctrl(Key.Down));

	/// <summary>Extend selection up one line (Shift+Up).</summary>
	public static RoutedUICommand SelectUpByLine { get; } = Cmd(nameof(SelectUpByLine), K(Key.Up, ModifierKeys.Shift));

	/// <summary>Extend selection down one line (Shift+Down).</summary>
	public static RoutedUICommand SelectDownByLine { get; } =
		Cmd(nameof(SelectDownByLine), K(Key.Down, ModifierKeys.Shift));

	/// <summary>Extend selection up one page (Shift+PageUp).</summary>
	public static RoutedUICommand SelectUpByPage { get; } =
		Cmd(nameof(SelectUpByPage), K(Key.PageUp, ModifierKeys.Shift));

	/// <summary>Extend selection down one page (Shift+PageDown).</summary>
	public static RoutedUICommand SelectDownByPage { get; } =
		Cmd(nameof(SelectDownByPage), K(Key.PageDown, ModifierKeys.Shift));

	/// <summary>Extend selection up one paragraph (Ctrl+Shift+Up).</summary>
	public static RoutedUICommand SelectUpByParagraph { get; } =
		Cmd(nameof(SelectUpByParagraph), K(Key.Up, CtrlShift));

	/// <summary>Extend selection down one paragraph (Ctrl+Shift+Down).</summary>
	public static RoutedUICommand SelectDownByParagraph { get; } =
		Cmd(nameof(SelectDownByParagraph), K(Key.Down, CtrlShift));

	/// <summary>Move to the line start (Home).</summary>
	public static RoutedUICommand MoveToLineStart { get; } = Cmd(nameof(MoveToLineStart), K(Key.Home));

	/// <summary>Move to the line end (End).</summary>
	public static RoutedUICommand MoveToLineEnd { get; } = Cmd(nameof(MoveToLineEnd), K(Key.End));

	/// <summary>Move to the document start (Ctrl+Home).</summary>
	public static RoutedUICommand MoveToDocumentStart { get; } = Cmd(nameof(MoveToDocumentStart), Ctrl(Key.Home));

	/// <summary>Move to the document end (Ctrl+End).</summary>
	public static RoutedUICommand MoveToDocumentEnd { get; } = Cmd(nameof(MoveToDocumentEnd), Ctrl(Key.End));

	/// <summary>Extend selection to the line start (Shift+Home).</summary>
	public static RoutedUICommand SelectToLineStart { get; } =
		Cmd(nameof(SelectToLineStart), K(Key.Home, ModifierKeys.Shift));

	/// <summary>Extend selection to the line end (Shift+End).</summary>
	public static RoutedUICommand SelectToLineEnd { get; } =
		Cmd(nameof(SelectToLineEnd), K(Key.End, ModifierKeys.Shift));

	/// <summary>Extend selection to the document start (Ctrl+Shift+Home).</summary>
	public static RoutedUICommand SelectToDocumentStart { get; } =
		Cmd(nameof(SelectToDocumentStart), K(Key.Home, CtrlShift));

	/// <summary>Extend selection to the document end (Ctrl+Shift+End).</summary>
	public static RoutedUICommand SelectToDocumentEnd { get; } =
		Cmd(nameof(SelectToDocumentEnd), K(Key.End, CtrlShift));
}
