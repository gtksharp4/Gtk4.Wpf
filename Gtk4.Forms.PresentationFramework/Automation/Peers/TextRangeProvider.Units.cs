using System.Collections.Generic;
using System.Windows.Automation.Text;

namespace System.Windows.Automation.Peers;

sealed partial class TextRangeProvider
{
	// Ascending offsets where a unit starts, ending with the text length.
	List<int> Boundaries(TextUnit unit)
	{
		var text = _owner.Text;
		var result = new List<int>();
		switch (unit)
		{
			case TextUnit.Character:
				for (int i = 0; i <= text.Length; i++) result.Add(i);
				return result;
			case TextUnit.Word:
				for (int i = 0; i < text.Length; i++)
					if (!char.IsWhiteSpace(text[i]) && (i == 0 || char.IsWhiteSpace(text[i - 1]))) result.Add(i);
				break;
			case TextUnit.Line:
			case TextUnit.Paragraph:
				result.Add(0);
				for (int i = 0; i < text.Length; i++)
					if (text[i] == '\n' && i + 1 < text.Length) result.Add(i + 1);
				break;
			default:
				result.Add(0);
				break;
		}
		if (result.Count == 0 || result[0] != 0) result.Insert(0, 0);
		if (result[result.Count - 1] != text.Length) result.Add(text.Length);
		return result;
	}

	// The unit boundary at or before the offset, and the one after it.
	static void Enclose(List<int> bounds, int offset, out int start, out int end)
	{
		int last = bounds.Count - 1;
		int i = 0;
		while (i < last && bounds[i + 1] <= offset) i++;
		start = bounds[i];
		end = bounds[Math.Min(i + 1, last)];
	}

	public void ExpandToEnclosingUnit(TextUnit unit)
	{
		Normalize();
		Enclose(Boundaries(unit), _start, out int start, out int end);
		_start = start;
		_end = end;
	}

	public int Move(TextUnit unit, int count)
	{
		if (count == 0) return 0;
		Normalize();
		var bounds = Boundaries(unit);
		Enclose(bounds, _start, out int start, out _);
		int index = bounds.IndexOf(start);
		int last = bounds.Count - 1;
		int target = Math.Max(0, Math.Min(index + count, Math.Max(0, last - 1)));
		bool degenerate = _start == _end;
		_start = bounds[target];
		_end = degenerate ? _start : bounds[Math.Min(target + 1, last)];
		return target - index;
	}

	public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
	{
		Normalize();
		var bounds = Boundaries(unit);
		int offset = Get(endpoint);
		int moved = 0;
		while (count > 0)
		{
			int next = bounds.Find(b => b > offset);
			if (next <= offset) break;
			offset = next;
			moved++;
			count--;
		}
		while (count < 0)
		{
			int previous = bounds.FindLast(b => b < offset);
			if (offset == 0 || previous >= offset) break;
			offset = previous;
			moved--;
			count++;
		}
		Set(endpoint, offset);
		return moved;
	}
}
