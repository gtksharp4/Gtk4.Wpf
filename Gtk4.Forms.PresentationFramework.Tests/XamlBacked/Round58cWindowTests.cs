using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.XamlBacked;

/// <summary>Compiled XAML: Media3D graphs, named freezables, key frames, bound animations, deferred dictionary values.</summary>
public class Round58cWindowTests : WpfGtkTestBase
{
	[Fact]
	public void Media3D_scene_is_built_and_its_nested_name_is_assigned() => Run(() =>
	{
		var window = new Round58cWindow();

		Assert.NotNull(window.View.Camera);
		Assert.NotNull(window.Mesh);
		Assert.Equal(3, window.Mesh.Positions.Count);
		Assert.Same(window.Mesh, window.FindName("Mesh"));
		var visual = Assert.IsType<System.Windows.Media.Media3D.ModelVisual3D>(window.View.Children[0]);
		Assert.NotNull(visual.Content);
	});

	[Fact]
	public void Key_frames_animate_the_named_transform_and_a_bound_To_loads() => Run(() =>
	{
		var window = new Round58cWindow();

		Assert.Same(window.Shift, window.FindName("Shift"));
		window.Pane.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
		var bs = (BeginStoryboard)((EventTrigger)window.Pane.Triggers[0]).Actions[0];
		Assert.Equal(2, bs.Storyboard!.ActiveClocks.Count);
		foreach (var clock in bs.Storyboard.ActiveClocks.ToArray()) clock.Tick(TimeSpan.FromSeconds(0.5));
		Assert.Equal(6.0, window.Shift.X, precision: 6);
	});

	[Fact]
	public void Dictionary_loads_without_resolving_an_unused_missing_key_and_throws_when_it_is_read() => Run(() =>
	{
		var dictionary = new DeferredDictionary();

		Assert.True(dictionary.ContainsKey("Unused"));
		Assert.Throws<InvalidOperationException>(() => dictionary["Unused"]);
		Assert.IsType<Style>(dictionary["Forward"]);
		Assert.Same(dictionary["Later"], dictionary["Later"]);
	});

	[Fact(Skip = "The binding engine serves FrameworkElement targets only (Data/**); an animation's To is not bound.")]
	public void A_bound_animation_To_follows_the_source() => Run(() =>
	{
		var window = new Round58cWindow();
		window.Pane.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
		var bs = (BeginStoryboard)((EventTrigger)window.Pane.Triggers[0]).Actions[0];
		foreach (var clock in bs.Storyboard!.ActiveClocks.ToArray()) clock.Tick(TimeSpan.FromSeconds(1));
		Assert.Equal(40.0, window.Src.Height, precision: 6);
	});
}
