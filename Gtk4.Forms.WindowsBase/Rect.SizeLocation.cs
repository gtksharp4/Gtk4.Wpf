using System;

namespace System.Windows;

public partial struct Rect
{
	/// <summary>The top-left corner. Setting it on <see cref="Empty"/> throws.</summary>
	/// <exception cref="InvalidOperationException">Set on the empty rectangle.</exception>
	public Point Location
	{
		get => new Point(X, Y);
		set
		{
			if (IsEmpty) throw new InvalidOperationException("Cannot modify this property on the Empty Rectangle.");
			X = value.X;
			Y = value.Y;
		}
	}

	/// <summary>The width and height. <see cref="Empty"/> reports <see cref="System.Windows.Size.Empty"/>.</summary>
	/// <exception cref="InvalidOperationException">Set on the empty rectangle.</exception>
	public Size Size
	{
		get => IsEmpty ? Size.Empty : new Size(Width, Height);
		set
		{
			if (IsEmpty) throw new InvalidOperationException("Cannot modify this property on the Empty Rectangle.");
			Width = value.Width;
			Height = value.Height;
		}
	}
}
