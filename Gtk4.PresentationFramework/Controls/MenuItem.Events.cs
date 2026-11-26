namespace System.Windows.Controls;

public partial class MenuItem
{
	/// <summary>Identifies the <see cref="Click"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
		nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));

	/// <summary>Identifies the <see cref="Checked"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
		nameof(Checked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));

	/// <summary>Identifies the <see cref="Unchecked"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
		nameof(Unchecked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));

	/// <summary>Raised when the item is activated (clicked).</summary>
	public event RoutedEventHandler Click
	{
		add => AddHandler(ClickEvent, value);
		remove => RemoveHandler(ClickEvent, value);
	}

	/// <summary>Raised when <see cref="IsChecked"/> becomes <c>true</c>.</summary>
	public event RoutedEventHandler Checked
	{
		add => AddHandler(CheckedEvent, value);
		remove => RemoveHandler(CheckedEvent, value);
	}

	/// <summary>Raised when <see cref="IsChecked"/> becomes <c>false</c>.</summary>
	public event RoutedEventHandler Unchecked
	{
		add => AddHandler(UncheckedEvent, value);
		remove => RemoveHandler(UncheckedEvent, value);
	}

	/// <summary>Raises <see cref="Click"/>, then executes <see cref="Command"/>.</summary>
	protected virtual void OnClick()
	{
		RaiseAutomationInvoked();
		RaiseEvent(new RoutedEventArgs(ClickEvent) { Source = this });
		if (CommandHelper.QueryCanExecute())
			CommandHelper.InvokeCommand();
	}

	/// <summary>Raises <see cref="Checked"/>.</summary>
	protected virtual void OnChecked(RoutedEventArgs e) => RaiseEvent(e);

	/// <summary>Raises <see cref="Unchecked"/>.</summary>
	protected virtual void OnUnchecked(RoutedEventArgs e) => RaiseEvent(e);
}
