using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// GTK application of <see cref="Slider"/> tick marks and position mapping
/// (split out of <c>Slider.Ticks.cs</c> to keep each file ≤200 lines).
/// </summary>
public partial class Slider
{
	/// <summary>
	/// Clears existing marks and adds new ones according to <see cref="TickPlacement"/>
	/// and <see cref="TickFrequency"/>. No-ops when <see cref="_scale"/> is null (pre-realize).
	/// </summary>
	/// <remarks>
	/// GTK limit: <see cref="Gtk.Scale.AddMark"/> places marks at absolute values;
	/// there is no GTK API to add marks uniformly — this method calls AddMark once per tick.
	/// For large ranges with tiny <see cref="TickFrequency"/> this can be slow; callers
	/// should keep TickFrequency at a reasonable size.
	/// </remarks>
	void ApplyTickProps()
	{
		if (_scale == null) return;

		// Update step/page increments.
		_scale.Adjustment.StepIncrement = SmallChange;
		_scale.Adjustment.PageIncrement = LargeChange;

		// Always clear existing marks first.
		_scale.ClearMarks();

		if (TickPlacement == TickPlacement.None)
			return;

		var gtkPos = ToGtkPositionType(TickPlacement, Orientation);
		foreach (var tick in TickValues())
			_scale.AddMark(tick, gtkPos, null);

		// When Both, add a second pass for the opposite side.
		if (TickPlacement == TickPlacement.Both)
		{
			var opp = OppositePosition(gtkPos);
			foreach (var tick in TickValues())
				_scale.AddMark(tick, opp, null);
		}
	}

	/// <summary>Maps <see cref="TickPlacement"/> + <see cref="Orientation"/> to a GTK position type.</summary>
	static Gtk.PositionType ToGtkPositionType(TickPlacement tp, Orientation orientation)
	{
		if (orientation == Orientation.Vertical)
		{
			// TopLeft → left side; BottomRight → right side; Both → start with left.
			return (tp == TickPlacement.BottomRight)
				? Gtk.PositionType.Right
				: Gtk.PositionType.Left;
		}

		// Horizontal: TopLeft → above track; BottomRight / Both → below track.
		return (tp == TickPlacement.TopLeft)
			? Gtk.PositionType.Top
			: Gtk.PositionType.Bottom;
	}

	static Gtk.PositionType OppositePosition(Gtk.PositionType pos)
	{
		switch (pos)
		{
			case Gtk.PositionType.Top: return Gtk.PositionType.Bottom;
			case Gtk.PositionType.Bottom: return Gtk.PositionType.Top;
			case Gtk.PositionType.Left: return Gtk.PositionType.Right;
			default: return Gtk.PositionType.Left;
		}
	}
}
