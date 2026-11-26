using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class WindowChromeTemplatePartTests : WpfGtkTestBase
{
	class Chip : Control
	{
	}

	static (Window Window, Chip Chip, TextBlock Part) Build(bool flag, bool? partValue = null,
		bool? styleValue = null, bool? borderValue = null)
	{
		TextBlock? part = null;
		var chip = new Chip
		{
			Width = 120,
			Height = 40,
			Template = new ControlTemplate
			{
				Factory = () =>
				{
					part = new TextBlock { Text = "chip", Width = 100, Height = 30 };
					if (partValue.HasValue) WindowChrome.SetIsHitTestVisibleInChrome(part, partValue.Value);
					if (styleValue.HasValue)
					{
						var style = new Style(typeof(TextBlock));
						style.Setters.Add(new Setter(WindowChrome.IsHitTestVisibleInChromeProperty, styleValue.Value));
						part.Style = style;
					}
					var border = new Border { Child = part };
					if (borderValue.HasValue) WindowChrome.SetIsHitTestVisibleInChrome(border, borderValue.Value);
					return border;
				},
			},
		};
		if (flag) WindowChrome.SetIsHitTestVisibleInChrome(chip, true);
		var w = new Window { Content = chip, Width = 300, Height = 200 };
		WindowChrome.SetWindowChrome(w, new WindowChrome { CaptionHeight = 100 });
		w.Show();
		return (w, chip, part!);
	}

	static bool AtPart(Window w, TextBlock part)
	{
		Assert.True(GtkPump.Until(() => part.Widget.IsMapped && part.Widget.Width > 0));
		Assert.True(part.Widget.ComputePoint(w.Widget, new Graphene.Point { X = 5, Y = 5 }, out var at));
		return w.IsHitTestVisibleInChromeAt(at.X, at.Y);
	}

	[Fact]
	public void A_template_part_takes_the_templated_controls_flag() => Run(() =>
	{
		var (w, chip, part) = Build(true);
		Assert.Same(chip, part.TemplatedParent);
		Assert.True(AtPart(w, part));
	});

	[Fact]
	public void A_template_part_without_the_flag_is_not_hit_test_visible() => Run(() =>
	{
		var (w, _, part) = Build(false);
		Assert.False(AtPart(w, part));
	});

	[Fact]
	public void An_explicit_false_on_a_part_is_kept() => Run(() =>
	{
		var (w, _, part) = Build(true, false);
		Assert.False(AtPart(w, part));
	});

	[Fact]
	public void A_style_set_true_on_a_part_of_an_unflagged_control_counts() => Run(() =>
	{
		var (w, _, part) = Build(false, styleValue: true);
		Assert.True(AtPart(w, part));
	});

	[Fact]
	public void A_style_set_false_on_a_part_of_a_flagged_control_counts() => Run(() =>
	{
		var (w, _, part) = Build(true, styleValue: false);
		Assert.False(AtPart(w, part));
	});

	[Fact]
	public void A_part_without_a_value_takes_its_in_template_ancestor() => Run(() =>
	{
		var (w, _, part) = Build(false, borderValue: true);
		Assert.True(AtPart(w, part));
	});

	[Fact]
	public void A_press_over_a_flagged_template_part_does_not_drag_but_plain_chrome_does() => Run(() =>
	{
		var (w, _, part) = Build(true);
		Assert.True(GtkPump.Until(() => part.Widget.IsMapped && part.Widget.Width > 0));
		Assert.True(part.Widget.ComputePoint(w.Widget, new Graphene.Point { X = 5, Y = 5 }, out var at));
		var moved = false;
		Window.BeginMoveOverride = (_, _, _, _, _) => moved = true;
		try
		{
			w.RecordPress(null, at.X, at.Y, 1);
			w.OnShellPressed(at.X, at.Y);
			Assert.False(moved);
			w.RecordPress(null, 280, 90, 1);
			w.OnShellPressed(280, 90);
			Assert.True(moved);
		}
		finally { Window.BeginMoveOverride = null; }
	});
}
