using System.Windows.Media;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Input and Commands/MousePointer": the border turns red while the pointer is over it.</summary>
public class MousePointerTests : WpfUiWindowTest<MousePointer.MainWindow>
{
	MousePointer.MainWindow Window => (MousePointer.MainWindow)App.Window;

	SolidColorBrush Background => App.Invoke(() => (SolidColorBrush)Window.border1.Background);

	[WpfUiFact]
	public void Entering_the_border_paints_it_red_and_leaving_paints_it_white()
	{
		App.Input.Mouse.MoveTo(Window.border1);
		Assert.Equal(Colors.Red, Background.Color);
		App.Input.Mouse.Leave(Window.border1);
		Assert.Equal(Colors.White, Background.Color);
	}

	[WpfUiFact]
	public void The_label_inside_the_border_is_found_by_its_text()
	{
		Assert.NotNull(App.TryFind(By.Name("Move Cursor Over Me")));
	}
}
