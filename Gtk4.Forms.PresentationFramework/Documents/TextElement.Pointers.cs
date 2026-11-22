namespace System.Windows.Documents;

public abstract partial class TextElement
{
	FlowDocument? _container;

	/// <summary>The FlowDocument that holds this element, or null when it is detached.</summary>
	internal FlowDocument? OwningDocument
	{
		get
		{
			DependencyObject? e = this;
			while (e is TextElement t) e = t.Parent;
			return e as FlowDocument;
		}
	}

	/// <summary>A pointer just before the start tag of this element (gravity Forward).</summary>
	public TextPointer ElementStart => Pointer(false, false, LogicalDirection.Forward);

	/// <summary>A pointer just after the start tag of this element, at its content (gravity Backward).</summary>
	public TextPointer ContentStart => Pointer(false, true, LogicalDirection.Backward);

	/// <summary>A pointer just before the end tag of this element, at its content end (gravity Forward).</summary>
	public TextPointer ContentEnd => Pointer(true, false, LogicalDirection.Forward);

	/// <summary>A pointer just after the end tag of this element (gravity Backward).</summary>
	public TextPointer ElementEnd => Pointer(true, true, LogicalDirection.Backward);

	/// <summary>
	/// The container the pointers live in: the owning FlowDocument, else a private one built on demand for the
	/// detached tree this element belongs to. <see cref="FrameworkContentElement.Parent"/> stays null. When the
	/// tree is later added to a document the private container is emptied, so pointers taken earlier read
	/// nothing instead of corrupting the new owner; ask for new ones.
	/// </summary>
	FlowDocument EnsureTextContainer()
	{
		TextElement root = this;
		for (;;)
		{
			if (root.Parent is FlowDocument doc) return doc;
			if (!(root.Parent is TextElement up)) break;
			root = up;
		}
		var container = root._container;
		if (container == null || !ReferenceEquals(container.DetachedRoot, root))
			root._container = container = new FlowDocument { DetachedRoot = root };
		return container;
	}

	internal void ReleaseTextContainer()
	{
		if (_container == null) return;
		_container.DetachedRoot = null;
		_container = null;
	}

	TextPointer Pointer(bool atEnd, bool afterTag, LogicalDirection direction)
	{
		var doc = EnsureTextContainer();
		var kind = atEnd ? TextSymKind.End : TextSymKind.Start;
		foreach (var sym in TextTree.Build(doc))
			if (sym.Kind == kind && ReferenceEquals(sym.Node, this))
				return new TextPointer(doc, sym.Offset + (afterTag ? 1 : 0), direction);
		throw new InvalidOperationException(
			"The element is not in a text container (table content has no text positions yet).");
	}
}
