using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Generated code resolves {StaticResource} through the enclosing parse scopes (Round 40).</summary>
public class StaticResourceScopeTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
	static readonly SolidColorBrush Blue = new SolidColorBrush(Colors.Blue);

	[Fact]
	public void Intermediate_ancestor_key_resolves_for_a_style_setter() => Run(() =>
	{
		var root = new Window();
		var grid = new Grid();
		grid.Resources["K"] = Red;
		var border = new Border();
		var style = new Style(typeof(Border));
		var value = StaticResourceScope.Find("K", root, border, grid);
		style.Setters.Add(new Setter { PropertyName = "Background", Value = value });
		border.Style = style;
		grid.Children.Add(border);
		root.Content = grid;
		Assert.Same(Red, border.Background);
	});

	[Fact]
	public void Enclosing_scope_wins_over_application_resources() => Run(() =>
	{
		var app = new Application();
		app.Resources["K"] = Blue;
		var root = new Window();
		var grid = new Grid();
		grid.Resources["K"] = Red;
		var border = new Border();
		Assert.Same(Red, StaticResourceScope.Find("K", root, border, grid));
	});

	[Fact]
	public void Innermost_scope_wins_then_root_then_application() => Run(() =>
	{
		var app = new Application();
		app.Resources["A"] = "app";
		var root = new Window();
		root.Resources["R"] = "root";
		var outer = new Grid();
		var inner = new Border();
		outer.Resources["K"] = "outer";
		inner.Resources["K"] = "inner";
		Assert.Equal("inner", StaticResourceScope.Find("K", root, inner, outer));
		Assert.Equal("root", StaticResourceScope.Find("R", root, inner, outer));
		Assert.Equal("app", StaticResourceScope.Find("A", root, inner, outer));
	});

	[Fact]
	public void Missing_key_throws_like_FindResource() => Run(() =>
	{
		var root = new Window();
		Assert.Throws<InvalidOperationException>(() => StaticResourceScope.Find("nope", root, new Grid()));
	});

	class GeneratedBase : Border
	{
		public int Downs;
		void OnDown(object s, MouseButtonEventArgs e) => Downs++;

		public Style CreateStyle()
		{
			var style = new Style(typeof(Border));
			var mi = typeof(GeneratedBase).GetMethod("OnDown",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			style.Setters.Add(new EventSetter(UIElement.MouseDownEvent,
				Delegate.CreateDelegate(UIElement.MouseDownEvent.HandlerType, this, mi!)));
			return style;
		}
	}

	sealed class Derived : GeneratedBase { }

	[Fact]
	public void Private_handler_binds_against_the_declaring_class_when_subclassed() => Run(() =>
	{
		var derived = new Derived();
		derived.Style = derived.CreateStyle();
		RoutedEventHost.RaiseEvent(derived,
			new MouseButtonEventArgs(UIElement.MouseDownEvent, new Point(0, 0), MouseButton.Left));
		Assert.Equal(1, derived.Downs);
		const BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		Assert.Null(typeof(Derived).GetMethod("OnDown", all));
	});
}
