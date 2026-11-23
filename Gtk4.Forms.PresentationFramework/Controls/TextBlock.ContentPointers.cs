using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class TextBlock
{
	TextBlockHost? _pointerHost;
	bool _pointerSync;

	/// <summary>A pointer at the start of the inline content (gravity Backward).</summary>
	/// <remarks>
	/// The pointers live in a private text container that mirrors <see cref="Inlines"/>; edits made through a
	/// <see cref="TextRange"/> are copied back. <see cref="TextPointer.Parent"/> is that container's root, not the
	/// TextBlock, because the text tree only walks flow elements.
	/// </remarks>
	public TextPointer ContentStart => EnsurePointerHost().ContentStart;

	/// <summary>A pointer at the end of the inline content (gravity Forward).</summary>
	public TextPointer ContentEnd => EnsurePointerHost().ContentEnd;

	TextBlockHost EnsurePointerHost()
	{
		if (_pointerHost != null) return _pointerHost;
		_ = Inlines;
		_pointerHost = new TextBlockHost(this);
		MirrorToHost();
		_pointerHost.Container.ContentEdited += OnHostEdited;
		return _pointerHost;
	}

	void SyncPointerHost()
	{
		if (_pointerHost != null && !_pointerSync) MirrorToHost();
	}

	void MirrorToHost()
	{
		_pointerSync = true;
		try
		{
			_pointerHost!.Inlines.Clear();
			foreach (var inline in _inlines)
			{
				_pointerHost.Inlines.Add(inline);
				inline.SetLogicalParent(this);
			}
		}
		finally { _pointerSync = false; }
	}

	void OnHostEdited()
	{
		if (_pointerSync || _pointerHost == null) return;
		_pointerSync = true;
		try
		{
			List<Inline> edited = _pointerHost.Inlines.ToList();
			if (!edited.SequenceEqual(_inlines))
			{
				_settingFromText = true;
				try
				{
					_inlines.Clear();
					foreach (var inline in edited) _inlines.Add(inline);
				}
				finally { _settingFromText = false; }
			}
			OnInlinesChanged();
		}
		finally { _pointerSync = false; }
	}
}
