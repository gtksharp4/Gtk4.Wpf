using System;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>
/// Parses the WPF Path Markup (SVG-compatible mini-language) string into a
/// <see cref="PathGeometry"/>.  Called by <see cref="Geometry.Parse"/>.
/// </summary>
internal static class PathMarkupParser
{
	/// <summary>Parses <paramref name="source"/> and returns a <see cref="PathGeometry"/>.</summary>
	/// <exception cref="FormatException">Unrecognised command or malformed number.</exception>
	internal static PathGeometry Parse(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
			return new PathGeometry();

		var pg = new PathGeometry();
		var ctx = new ParseContext();

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
		ParseContext ctx, PathGeometry pg)
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
		ParseContext ctx, PathGeometry pg)
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

	static void ParseLines(string s, ref int i, bool rel, ParseContext ctx)
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
		ParseContext ctx)
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

	static void ParseCubics(string s, ref int i, bool rel, ParseContext ctx)
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

	static void ParseQuadratics(string s, ref int i, bool rel, ParseContext ctx)
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

	// ── lexer helpers ─────────────────────────────────────────────────────────

	static void SkipWs(string s, ref int i)
	{
		while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n'))
			i++;
	}

	static void SkipSep(string s, ref int i)
	{
		// optional whitespace, optional comma, optional whitespace
		SkipWs(s, ref i);
		if (i < s.Length && s[i] == ',') { i++; SkipWs(s, ref i); }
	}

	static bool IsNumberStart(char c) =>
		char.IsDigit(c) || c == '-' || c == '+' || c == '.';

	static double ReadNumber(string s, ref int i)
	{
		int start = i;
		if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++;
		while (i < s.Length && char.IsDigit(s[i])) i++;
		if (i < s.Length && s[i] == '.')
		{
			i++;
			while (i < s.Length && char.IsDigit(s[i])) i++;
		}
		if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
		{
			i++;
			if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
			while (i < s.Length && char.IsDigit(s[i])) i++;
		}
		if (i == start)
			throw new FormatException($"Expected a number at position {start} in path data.");
		return double.Parse(s.AsSpan(start, i - start), CultureInfo.InvariantCulture);
	}

	// ── parse state ───────────────────────────────────────────────────────────

	sealed class ParseContext
	{
		PathFigure? _figure;
		public double CurX { get; private set; }
		public double CurY { get; private set; }

		public void StartFigure(double x, double y)
		{
			_figure = new PathFigure { StartPoint = new Point(x, y) };
			CurX = x; CurY = y;
		}

		public void AddLine(double x, double y)
		{
			EnsureFigure(x, y);
			_figure!.Segments.Add(new LineSegment(new Point(x, y)));
			CurX = x; CurY = y;
		}

		public void AddCubic(Point p1, Point p2, Point p3)
		{
			EnsureFigure(p3.X, p3.Y);
			_figure!.Segments.Add(new BezierSegment(p1, p2, p3));
			CurX = p3.X; CurY = p3.Y;
		}

		public void AddQuadratic(Point p1, Point p2)
		{
			EnsureFigure(p2.X, p2.Y);
			_figure!.Segments.Add(new QuadraticBezierSegment(p1, p2));
			CurX = p2.X; CurY = p2.Y;
		}

		public void CloseFigure(PathGeometry pg)
		{
			if (_figure != null) { _figure.IsClosed = true; FlushFigure(pg); }
		}

		public void FlushFigure(PathGeometry pg)
		{
			if (_figure != null) { pg.Figures.Add(_figure); _figure = null; }
		}

		void EnsureFigure(double x, double y)
		{
			if (_figure == null) StartFigure(x, y);
		}
	}
}
