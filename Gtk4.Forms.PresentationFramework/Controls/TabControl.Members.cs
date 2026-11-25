namespace System.Windows.Controls;

public partial class TabControl
{
	/// <summary>Identifies <see cref="TabStripPlacement"/>. Default <see cref="Dock.Top"/>.</summary>
	public static readonly DependencyProperty TabStripPlacementProperty = DependencyProperty.Register(
		nameof(TabStripPlacement), typeof(Dock), typeof(TabControl),
		new FrameworkPropertyMetadata(Dock.Top, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPlacementChanged),
		v => v is Dock d && d >= Dock.Left && d <= Dock.Bottom);

	/// <summary>Gets or sets the side of the control on which the tab strip is shown.</summary>
	public Dock TabStripPlacement
	{
		get => (Dock)GetValue(TabStripPlacementProperty)!;
		set => SetValue(TabStripPlacementProperty, value);
	}

	static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tabs = (TabControl)d;
		if (tabs._notebook != null) tabs._notebook.TabPos = ToPosition((Dock)e.NewValue!);
	}

	static Gtk.PositionType ToPosition(Dock dock) => dock switch
	{
		Dock.Left => Gtk.PositionType.Left,
		Dock.Right => Gtk.PositionType.Right,
		Dock.Bottom => Gtk.PositionType.Bottom,
		_ => Gtk.PositionType.Top,
	};

	// Keeps TabItem.IsSelected in step with the selection, whichever path changed it.
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (HasContentTemplating && (e.Property == SelectedIndexProperty || e.Property == SelectedItemProperty))
			RefreshTabContent();
		if (e.Property != SelectedItemProperty) return;
		if (e.OldValue is TabItem old) old.SetValue(TabItem.IsSelectedProperty, false);
		if (e.NewValue is TabItem item) item.SetValue(TabItem.IsSelectedProperty, true);
	}
}
