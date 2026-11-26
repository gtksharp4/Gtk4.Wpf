namespace System.Windows.Controls.Primitives;

public partial class ToggleButton
{
	/// <summary>Identifies the <see cref="Checked"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
		nameof(Checked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));

	/// <summary>Identifies the <see cref="Unchecked"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
		nameof(Unchecked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));

	/// <summary>Identifies the <see cref="Indeterminate"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent IndeterminateEvent = EventManager.RegisterRoutedEvent(
		nameof(Indeterminate), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleButton));

	/// <summary>Occurs when <see cref="IsChecked"/> becomes <c>true</c>.</summary>
	public event RoutedEventHandler Checked
	{
		add => AddHandler(CheckedEvent, value);
		remove => RemoveHandler(CheckedEvent, value);
	}

	/// <summary>Occurs when <see cref="IsChecked"/> becomes <c>false</c>.</summary>
	public event RoutedEventHandler Unchecked
	{
		add => AddHandler(UncheckedEvent, value);
		remove => RemoveHandler(UncheckedEvent, value);
	}

	/// <summary>Occurs when <see cref="IsChecked"/> becomes <c>null</c>.</summary>
	public event RoutedEventHandler Indeterminate
	{
		add => AddHandler(IndeterminateEvent, value);
		remove => RemoveHandler(IndeterminateEvent, value);
	}

	/// <summary>Raises <see cref="Checked"/>.</summary>
	protected virtual void OnChecked(RoutedEventArgs e) => RaiseEvent(e);

	/// <summary>Raises <see cref="Unchecked"/>.</summary>
	protected virtual void OnUnchecked(RoutedEventArgs e) => RaiseEvent(e);

	/// <summary>Raises <see cref="Indeterminate"/>.</summary>
	protected virtual void OnIndeterminate(RoutedEventArgs e) => RaiseEvent(e);

	/// <summary>Identifies the <see cref="IsChecked"/> property.</summary>
	public static readonly DependencyProperty IsCheckedProperty =
		DependencyProperty.Register(
			nameof(IsChecked), typeof(bool?), typeof(ToggleButton),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
				OnIsCheckedChanged));

	/// <summary>Identifies the <see cref="IsThreeState"/> property.</summary>
	public static readonly DependencyProperty IsThreeStateProperty =
		DependencyProperty.Register(nameof(IsThreeState), typeof(bool), typeof(ToggleButton),
			new PropertyMetadata(false));

	/// <summary>Gets or sets the check state; <c>null</c> is indeterminate.</summary>
	public bool? IsChecked
	{
		get => (bool?)GetValue(IsCheckedProperty);
		set => SetValue(IsCheckedProperty, value);
	}

	/// <summary>Gets or sets whether the control cycles through the indeterminate state.</summary>
	public bool IsThreeState
	{
		get => (bool?)GetValue(IsThreeStateProperty) == true;
		set => SetValue(IsThreeStateProperty, value);
	}

	static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tb = (ToggleButton)d;
		var value = e.NewValue as bool?;
		tb.SyncGtk(value == true);
		if (value == true)
			tb.OnChecked(new RoutedEventArgs(CheckedEvent) { Source = tb });
		else if (value == false)
			tb.OnUnchecked(new RoutedEventArgs(UncheckedEvent) { Source = tb });
		else
			tb.OnIndeterminate(new RoutedEventArgs(IndeterminateEvent) { Source = tb });
	}

	/// <summary>Advances the state: checked to unchecked (or indeterminate when three-state), else checked.</summary>
	protected virtual void OnToggle()
	{
		bool? next;
		if (IsChecked == true)
			next = IsThreeState ? (bool?)null : false;
		else
			next = IsChecked.HasValue;
		SetValue(IsCheckedProperty, next);
	}
}
