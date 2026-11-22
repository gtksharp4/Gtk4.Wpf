using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Annotations;

public sealed partial class AnnotationService
{
	readonly List<CommandBinding> _bindings = new List<CommandBinding>();

	// Create and clear need a non-empty selection; the delete commands only need one to exist (a caret).
	void AddCommandBindings()
	{
		Bind(CreateHighlightCommand, true,
			e => AnnotationHelper.CreateHighlightForSelection(this, null, e.Parameter as Brush));
		Bind(CreateTextStickyNoteCommand, true,
			e => AnnotationHelper.CreateTextStickyNoteForSelection(this, e.Parameter as string));
		Bind(CreateInkStickyNoteCommand, true,
			e => AnnotationHelper.CreateInkStickyNoteForSelection(this, e.Parameter as string));
		Bind(ClearHighlightsCommand, true, e => AnnotationHelper.ClearHighlightsForSelection(this));
		Bind(DeleteStickyNotesCommand, false, e =>
		{
			AnnotationHelper.DeleteTextStickyNotesForSelection(this);
			AnnotationHelper.DeleteInkStickyNotesForSelection(this);
		});
		Bind(DeleteAnnotationsCommand, false, e =>
		{
			AnnotationHelper.DeleteTextStickyNotesForSelection(this);
			AnnotationHelper.DeleteInkStickyNotesForSelection(this);
			if (Selection is { IsEmpty: false }) AnnotationHelper.ClearHighlightsForSelection(this);
		});
	}

	void Bind(RoutedUICommand command, bool needsText, Action<ExecutedRoutedEventArgs> run)
	{
		var binding = new CommandBinding(command,
			(s, e) =>
			{
				if (!CanRun(needsText)) return;
				run(e);
				e.Handled = true;
			},
			(s, e) =>
			{
				e.CanExecute = CanRun(needsText);
				e.Handled = true;
			});
		_bindings.Add(binding);
		_host.Element.CommandBindings.Add(binding);
	}

	bool CanRun(bool needsText)
	{
		var selection = Selection;
		return _enabled && selection != null && (!needsText || !selection.IsEmpty);
	}

	void RemoveCommandBindings()
	{
		foreach (var binding in _bindings) _host.Element.CommandBindings.Remove(binding);
		_bindings.Clear();
	}
}
