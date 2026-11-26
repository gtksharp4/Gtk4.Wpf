using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Round 49a wrap-up: OnApplyTemplate, LayoutUpdated lifetime, PageFunction and StartupUri hosting.</summary>
public class Round49aWrapElementTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	sealed class Plain : FrameworkElement
	{
		public override void OnApplyTemplate() { }

		protected override Gtk.Widget CreateWidget() => throw new NotSupportedException("pure test");
	}

	sealed class Fn : PageFunction<int>
	{
	}

	[Fact]
	public void OnApplyTemplate_is_declared_on_FrameworkElement()
	{
		var method = typeof(FrameworkElement).GetMethod("OnApplyTemplate")!;
		Assert.Equal(typeof(FrameworkElement), method.DeclaringType);
		Assert.Equal(typeof(FrameworkElement), typeof(Control).GetMethod("OnApplyTemplate")!.DeclaringType);
		Assert.NotNull(new Plain());
	}

	[Fact]
	[MethodImpl(MethodImplOptions.NoInlining)]
	public void LayoutUpdated_subscription_does_not_keep_the_element_alive()
	{
		WeakReference weak = Subscribe();
		for (int i = 0; i < 3 && weak.IsAlive; i++)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
		Assert.False(weak.IsAlive);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference Subscribe()
	{
		var el = new Plain();
		el.LayoutUpdated += OnLayoutUpdated;
		return new WeakReference(el);
	}

	static void OnLayoutUpdated(object? sender, EventArgs e) { }

	[Fact]
	public void PageFunction_has_a_public_constructor_and_a_family_Start()
	{
		var ctor = typeof(PageFunction<int>).GetConstructor(Type.EmptyTypes);
		Assert.True(ctor!.IsPublic);
		var start = typeof(PageFunctionBase).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)!;
		Assert.True(start.IsFamily);
		Assert.NotNull(new Fn());
	}

	[Fact]
	public void Non_window_startup_root_is_hosted_in_a_NavigationWindow() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Pane49.xaml", () => new StackPanel());
		var app = new RecordingApp { StartupUri = new Uri("Pane49.xaml", UriKind.Relative) };
		Window? main = null;
		Visibility vis = Visibility.Collapsed;
		app.RunLoopOverride = a => { main = a.MainWindow; vis = main!.Visibility; };

		app.RunCore(new string[0]);

		var nav = Assert.IsType<NavigationWindow>(main);
		Assert.IsType<StackPanel>(nav.Content);
		Assert.Equal(Visibility.Visible, vis);
	});
}
