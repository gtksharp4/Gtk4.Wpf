using System;

namespace System.Windows.Media;

/// <summary>
/// Parses the WPF Path Markup (SVG-compatible mini-language) string into a
/// <see cref="PathGeometry"/>.  Called by <see cref="Geometry.Parse"/>.
/// <para>
/// Supported commands (absolute + relative): <c>M L H V C Q Z</c> with implicit
/// repeated coordinate sets. <c>A</c> (arc), <c>S</c>/<c>T</c> (smooth) are parsed
/// but skipped (best-effort — their parameters are consumed so the rest of the string
/// still parses). Lexer helpers live in <c>PathMarkupParser.Lexer.cs</c>; the mutable
/// build state lives in <see cref="PathParseContext"/>.
/// </para>
/// </summary>
internal static partial class PathMarkupParser
{
	/// <summary>Parses <paramref name="source"/> and returns a <see cref="PathGeometry"/>.</summary>
	/// <exception cref="FormatException">Unrecognised command or malformed number.</exception>
	internal static PathGeometry Parse(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
			return new PathGeometry();

		var pg = new PathGeometry();
		var ctx = new PathParseContext();

		int i = 0;
		SkipWs(source, ref i);
		while (i < source.Length)
		{
			char cmd = source[i++];
			SkipWs(source, ref i);
			DispatchCommand(source, ref i, cmd, ctx, pg);
			SkipWs(source, ref i);
		}
		ctx.FlushFigure(pg);
		return pg;
	}

	// ── dispatch ──────────────────────────────────────────────────────────────

	static void DispatchCommand(string s, ref int i, char cmd,
		PathParseContext ctx, PathGeometry pg)
	{
		bool rel = char.IsLower(cmd);
		switch (char.ToUpperInvariant(cmd))
		{
			case 'M': ParseMove(s, ref i, rel, ctx, pg); break;
			case 'L': ParseLines(s, ref i, rel, ctx); break;
			case 'H': ParseHV(s, ref i, rel, true, ctx); break;
			case 'V': ParseHV(s, ref i, rel, false, ctx); break;
			case 'C': ParseCubics(s, ref i, rel, ctx); break;
			case 'Q': ParseQuadratics(s, ref i, rel, ctx); break;
			case 'Z': ctx.CloseFigure(pg); break;
			case 'A': SkipArcParams(s, ref i); break;   // best-effort: skip
			case 'S': SkipPairs(s, ref i, 4); break;    // best-effort: skip
			case 'T': SkipPairs(s, ref i, 2); break;    // best-effort: skip
			default: throw new FormatException($"Unknown path command '{cmd}'.");
		}
	}

	// ── command handlers ──────────────────────────────────────────────────────

	static void ParseMove(string s, ref int i, bool rel,
		PathParseContext ctx, PathGeometry pg)
	{
		ctx.FlushFigure(pg);
		double x = ReadNumber(s, ref i);
		SkipSep(s, ref i);
		double y = ReadNumber(s, ref i);
		ctx.StartFigure(rel ? ctx.CurX + x : x, rel ? ctx.CurY + y : y);
		// Implicit L after M
		SkipSep(s, ref i);
		if (i < s.Length && (s[i] == '-' || s[i] == '+' || char.IsDigit(s[i]) || s[i] == '.'))
			ParseLines(s, ref i, rel, ctx);
	}

	static void ParseLines(string s, ref int i, bool rel, PathParseContext ctx)
	{
		do
		{
			double x = ReadNumber(s, ref i);
			SkipSep(s, ref i);
			double y = ReadNumber(s, ref i);
			double ax = rel ? ctx.CurX + x : x;
			double ay = rel ? ctx.CurY + y : y;
			ctx.AddLine(ax, ay);
			SkipSep(s, ref i);
		} while (i < s.Length && IsNumberStart(s[i]));
	}

	static void ParseHV(string s, ref int i, bool rel, bool horizontal,
		PathParseContext ctx)
	{
		do
		{
			double v = ReadNumber(s, ref i);
			double ax = horizontal ? (rel ? ctx.CurX + v : v) : ctx.CurX;
			double ay = horizontal ? ctx.CurY : (rel ? ctx.CurY + v : v);
			ctx.AddLine(ax, ay);
			SkipSep(s, ref i);
		} while (i < s.Length && IsNumberStart(s[i]));
	}

	static void ParseCubics(string s, ref int i, bool rel, PathParseContext ctx)
	{
		do
		{
			double x1 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double y1 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double x2 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double y2 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double x  = ReadNumber(s, ref i); SkipSep(s, ref i);
			double y  = ReadNumber(s, ref i);
			double bx = rel ? ctx.CurX : 0, by = rel ? ctx.CurY : 0;
			ctx.AddCubic(
				new Point(bx + x1, by + y1),
				new Point(bx + x2, by + y2),
				new Point(bx + x,  by + y));
			SkipSep(s, ref i);
		} while (i < s.Length && IsNumberStart(s[i]));
	}

	static void ParseQuadratics(string s, ref int i, bool rel, PathParseContext ctx)
	{
		do
		{
			double x1 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double y1 = ReadNumber(s, ref i); SkipSep(s, ref i);
			double x  = ReadNumber(s, ref i); SkipSep(s, ref i);
			double y  = ReadNumber(s, ref i);
			double bx = rel ? ctx.CurX : 0, by = rel ? ctx.CurY : 0;
			ctx.AddQuadratic(new Point(bx + x1, by + y1), new Point(bx + x, by + y));
			SkipSep(s, ref i);
		} while (i < s.Length && IsNumberStart(s[i]));
	}

	// best-effort skips ───────────────────────────────────────────────────────

	static void SkipArcParams(string s, ref int i)
	{
		// A: rx ry x-rotation large-arc-flag sweep-flag x y  (7 numbers)
		while (i < s.Length && IsNumberStart(s[i]))
		{
			for (int p = 0; p < 7 && i < s.Length && IsNumberStart(s[i]); p++)
			{ ReadNumber(s, ref i); SkipSep(s, ref i); }
		}
	}

	static void SkipPairs(string s, ref int i, int count)
	{
		while (i < s.Length && IsNumberStart(s[i]))
		{
			for (int p = 0; p < count && i < s.Length && IsNumberStart(s[i]); p++)
			{ ReadNumber(s, ref i); SkipSep(s, ref i); }
		}
	}
}
