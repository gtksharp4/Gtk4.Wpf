using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// GTK smoke tests for <see cref="System.Windows.FrameworkElement.Cursor"/>.
/// Skipped when GTK is unavailable.
/// </summary>
public class CursorGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Cursor_set_before_realize_applied_on_widget_creation() => Run(() =>
	{
		var tb = new TextBlock { Text = "hover" };
		tb.Cursor = Cursors.Hand;

		// Materialize — must not throw.
		var widget = tb.Widget;
		Assert.NotNull(widget);

		// DP value must survive widget creation.
		Assert.NotNull(tb.Cursor);
		Assert.Equal("pointer", tb.Cursor!.GdkName);
	});

	[Fact]
	public void Cursor_set_after_realize_does_not_throw() => Run(() =>
	{
		var tb = new TextBlock { Text = "move" };
		_ = tb.Widget;

		var ex = Record.Exception(() => tb.Cursor = Cursors.SizeAll);
		Assert.Null(ex);
		Assert.Equal("move", tb.Cursor!.GdkName);
	});

	[Fact]
	public void Cursor_clear_after_realize_does_not_throw() => Run(() =>
	{
		var tb = new TextBlock { Text = "clear" };
		tb.Cursor = Cursors.Wait;
		_ = tb.Widget;

		var ex = Record.Exception(() => tb.Cursor = null);
		Assert.Null(ex);
		Assert.Null(tb.Cursor);
	});

	[Fact]
	public void Realized_element_with_Hand_cursor_has_nonnull_widget_Cursor() => Run(() =>
	{
		var tb = new TextBlock { Text = "ptr" };
		tb.Cursor = Cursors.Hand;
		var widget = tb.Widget;

		// gtk_widget_set_cursor_from_name sets widget.Cursor; verify it is non-null.
		Assert.NotNull(widget.Cursor);
	});
}
