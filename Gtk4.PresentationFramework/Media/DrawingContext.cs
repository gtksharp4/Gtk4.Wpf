using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Records drawing commands issued by <see cref="DrawingVisual.RenderOpen"/>.
/// Mirrors <c>System.Windows.Media.DrawingContext</c>.
/// </summary>
/// <remarks>
/// This is a minimal recording context. Actual Cairo/GTK rendering is a Phase-2 item;
/// the recorded commands are available for inspection in tests via
/// <see cref="Commands"/>.
/// </remarks>
public sealed class DrawingContext : IDisposable
{
	/// <summary>Discriminator for recorded draw operations.</summary>
	public enum DrawCommandKind { DrawRectangle, DrawEllipse, DrawLine, DrawGeometry, DrawText }

	/// <summary>A single recorded draw command.</summary>
	public sealed class DrawCommand(
		DrawCommandKind Kind,
		Brush? Brush,
		Pen? Pen,
		object? Arg)
	{
		public DrawCommandKind Kind { get; } = Kind;
		public Brush? Brush { get; } = Brush;
		public Pen? Pen { get; } = Pen;
		public object? Arg { get; } = Arg;

		public void Deconstruct(out DrawCommandKind Kind, out Brush? Brush, out Pen? Pen, out object? Arg)
		{
			Kind = this.Kind;
			Brush = this.Brush;
			Pen = this.Pen;
			Arg = this.Arg;
		}
	}

	readonly List<DrawCommand> _commands = new();

	/// <summary>All commands recorded so far.</summary>
	public IReadOnlyList<DrawCommand> Commands => _commands;

	// ── Draw primitives ───────────────────────────────────────────────────────────

	/// <summary>Records a filled/outlined rectangle.</summary>
	public void DrawRectangle(Brush? brush, Pen? pen, System.Windows.Rect rect) =>
		_commands.Add(new DrawCommand(DrawCommandKind.DrawRectangle, brush, pen, rect));

	/// <summary>Records a filled/outlined ellipse.</summary>
	public void DrawEllipse(
		Brush? brush, Pen? pen,
		System.Windows.Point center,
		double radiusX, double radiusY) =>
		_commands.Add(new DrawCommand(DrawCommandKind.DrawEllipse, brush, pen,
			(center, radiusX, radiusY)));

	/// <summary>Records a line.</summary>
	public void DrawLine(Pen pen, System.Windows.Point point0, System.Windows.Point point1) =>
		_commands.Add(new DrawCommand(DrawCommandKind.DrawLine, null, pen, (point0, point1)));

	/// <summary>Records a geometry fill/stroke.</summary>
	public void DrawGeometry(Brush? brush, Pen? pen, Geometry? geometry) =>
		_commands.Add(new DrawCommand(DrawCommandKind.DrawGeometry, brush, pen, geometry));

	/// <summary>Records a text draw (placeholder).</summary>
	public void DrawText(FormattedText formattedText, System.Windows.Point origin) =>
		_commands.Add(new DrawCommand(DrawCommandKind.DrawText, null, null,
			(formattedText, origin)));

	// ── Lifecycle ─────────────────────────────────────────────────────────────────

	Action<DrawingContext>? _onClose;
	bool _closed;

	/// <summary>Creates a free-standing recording context.</summary>
	public DrawingContext() { }

	internal DrawingContext(Action<DrawingContext> onClose) => _onClose = onClose;

	/// <summary>Lets code written against the old scope type (<c>scope.Context</c>) compile unchanged.</summary>
	internal DrawingContext Context => this;

	/// <summary>Seals the context and commits it to the owning visual. A second call does nothing.</summary>
	public void Close()
	{
		if (_closed) return;
		_closed = true;
		_onClose?.Invoke(this);
	}

	/// <inheritdoc/>
	public void Dispose() => Close();
}

