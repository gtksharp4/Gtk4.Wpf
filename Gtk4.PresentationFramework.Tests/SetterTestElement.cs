using System;
using System.Windows;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Widget-free element shared by the Setter value and EventSetter tests.</summary>
sealed class SetterTestElement : FrameworkElement
{
	public new static readonly DependencyProperty TagProperty =
		DependencyProperty.Register("SetterTestTag", typeof(string), typeof(SetterTestElement),
			new PropertyMetadata(null));

	public static readonly DependencyProperty FlagProperty =
		DependencyProperty.Register("SetterTestFlag", typeof(bool), typeof(SetterTestElement),
			new PropertyMetadata(false));

	public static readonly RoutedEvent PingEvent =
		EventManager.RegisterRoutedEvent("SetterTestPing", RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(SetterTestElement));

	public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
	public bool Flag { get => (bool)GetValue(FlagProperty)!; set => SetValue(FlagProperty, value); }
	public void Ping() => RaiseEvent(new RoutedEventArgs(PingEvent));

	protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
}

sealed class SetterTestSource
{
	public string Name { get; set; } = "";
}
