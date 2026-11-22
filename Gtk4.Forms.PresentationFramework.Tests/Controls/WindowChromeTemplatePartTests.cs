using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class WindowChromeTemplatePartTests : WpfGtkTestBase
{
	class Chip : Control
	{
	}

	static (Window Window, Chip Chip, TextBlock Part) Build(bool flag, bool? partValue = null)
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
					return new Border { Child = part };
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
}
