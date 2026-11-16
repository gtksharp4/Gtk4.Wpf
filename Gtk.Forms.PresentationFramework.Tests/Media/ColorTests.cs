using System;
using System.Windows.Media;
using Xunit;

namespace Gtk.Forms.PresentationFramework.Tests.Media
{
	/// <summary>
	/// Pure tests for <see cref="Color"/>, <see cref="Colors"/>, <see cref="Brushes"/>
	/// and <see cref="SolidColorBrush.ToGdkRgba"/>.
	/// </summary>
	public class ColorTests
	{
		// ── Color.FromRgb ────────────────────────────────────────────────────────

		[Fact]
		public void FromRgb_sets_A_to_255()
		{
			var c = Color.FromRgb(10, 20, 30);
			Assert.Equal(255, c.A);
		}

		[Fact]
		public void FromRgb_stores_rgb_bytes()
		{
			var c = Color.FromRgb(1, 2, 3);
			Assert.Equal(1, c.R);
			Assert.Equal(2, c.G);
			Assert.Equal(3, c.B);
		}

		[Fact]
		public void FromArgb_stores_all_channels()
		{
			var c = Color.FromArgb(100, 50, 60, 70);
			Assert.Equal(100, c.A);
			Assert.Equal(50, c.R);
			Assert.Equal(60, c.G);
			Assert.Equal(70, c.B);
		}

		// ── Equality ─────────────────────────────────────────────────────────────

		[Fact]
		public void Same_argb_values_are_equal()
		{
			var a = Color.FromArgb(255, 100, 150, 200);
			var b = Color.FromArgb(255, 100, 150, 200);
			Assert.Equal(a, b);
		}

		[Fact]
		public void Different_alpha_are_not_equal()
		{
			var a = Color.FromArgb(255, 100, 150, 200);
			var b = Color.FromArgb(128, 100, 150, 200);
			Assert.NotEqual(a, b);
		}

		[Fact]
		public void Equality_operator_matches_Equals()
		{
			var a = Color.FromRgb(255, 0, 128);
			var b = Color.FromRgb(255, 0, 128);
			Assert.True(a == b);
			Assert.False(a != b);
		}

		// ── ScA / ScR / ScG / ScB ────────────────────────────────────────────────

		[Fact]
		public void ScA_of_opaque_colour_is_one()
		{
			var c = Color.FromRgb(0, 0, 0);
			Assert.Equal(1f, c.ScA, precision: 3);
		}

		[Fact]
		public void ScR_of_full_red_is_one()
		{
			var c = Color.FromRgb(255, 0, 0);
			Assert.Equal(1f, c.ScR, precision: 3);
		}

		// ── F7: Sc* are normalized byte-channel values (NOT linear scRGB) ────────

		/// <summary>
		/// Sc* properties normalize the byte channel to [0,1] (channel / 255).
		/// They do NOT apply the sRGB⇄linear-light transfer function; that is
		/// documented in the XML doc comments and reserved for a future phase.
		/// </summary>
		[Theory]
		[InlineData(0,   0f)]
		[InlineData(128, 128f / 255f)]
		[InlineData(255, 1f)]
		public void ScR_is_byte_normalized(byte r, float expected)
		{
			var c = Color.FromRgb(r, 0, 0);
			Assert.Equal(expected, c.ScR, precision: 3);
		}

		[Theory]
		[InlineData(0,   0f)]
		[InlineData(64,  64f / 255f)]
		[InlineData(255, 1f)]
		public void ScG_is_byte_normalized(byte g, float expected)
		{
			var c = Color.FromRgb(0, g, 0);
			Assert.Equal(expected, c.ScG, precision: 3);
		}

		[Theory]
		[InlineData(0,   0f)]
		[InlineData(192, 192f / 255f)]
		[InlineData(255, 1f)]
		public void ScB_is_byte_normalized(byte b, float expected)
		{
			var c = Color.FromRgb(0, 0, b);
			Assert.Equal(expected, c.ScB, precision: 3);
		}

		[Theory]
		[InlineData(0,   0f)]
		[InlineData(128, 128f / 255f)]
		[InlineData(255, 1f)]
		public void ScA_is_byte_normalized(byte a, float expected)
		{
			var c = Color.FromArgb(a, 0, 0, 0);
			Assert.Equal(expected, c.ScA, precision: 3);
		}

		[Fact]
		public void FromScRgb_is_byte_normalization_not_linear_light()
		{
			// 0.5f * 255 rounds to 128; this is the documented normalized behaviour,
			// not the sRGB⇄linear transfer (which would give ~186 for 0.5f).
			var c = Color.FromScRgb(1f, 0.5f, 0.5f, 0.5f);
			Assert.Equal(255, c.A);
			Assert.Equal(128, c.R);
			Assert.Equal(128, c.G);
			Assert.Equal(128, c.B);
		}

		[Fact]
		public void ScA_setter_roundtrips_through_byte()
		{
			var c = Color.FromArgb(0, 0, 0, 0);
			c.ScA = 0.5f;
			// 0.5 * 255 = 127.5 → rounds to 128
			Assert.Equal(128, c.A);
		}

		// ── ToGdkRgba ────────────────────────────────────────────────────────────

		[Fact]
		public void ToGdkRgba_white_gives_all_ones()
		{
			var c = Colors.White;
			var rgba = c.ToGdkRgba();
			Assert.Equal(1f, rgba.Red, precision: 3);
			Assert.Equal(1f, rgba.Green, precision: 3);
			Assert.Equal(1f, rgba.Blue, precision: 3);
			Assert.Equal(1f, rgba.Alpha, precision: 3);
		}

		[Fact]
		public void ToGdkRgba_black_gives_rgb_zero()
		{
			var c = Colors.Black;
			var rgba = c.ToGdkRgba();
			Assert.Equal(0f, rgba.Red, precision: 3);
			Assert.Equal(0f, rgba.Green, precision: 3);
			Assert.Equal(0f, rgba.Blue, precision: 3);
		}

		[Fact]
		public void ToGdkRgba_transparent_gives_alpha_zero()
		{
			var c = Colors.Transparent;
			var rgba = c.ToGdkRgba();
			Assert.Equal(0f, rgba.Alpha, precision: 3);
		}

		// ── Colors named values ──────────────────────────────────────────────────

		[Fact]
		public void Colors_Red_has_full_red_channel()
		{
			Assert.Equal(255, Colors.Red.R);
			Assert.Equal(0, Colors.Red.G);
			Assert.Equal(0, Colors.Red.B);
		}

		[Fact]
		public void Colors_Blue_has_full_blue_channel()
		{
			Assert.Equal(0, Colors.Blue.R);
			Assert.Equal(0, Colors.Blue.G);
			Assert.Equal(255, Colors.Blue.B);
		}

		// ── SolidColorBrush / Brushes ────────────────────────────────────────────

		[Fact]
		public void SolidColorBrush_ToGdkRgba_matches_color()
		{
			var brush = new SolidColorBrush(Colors.Red);
			var rgba = brush.ToGdkRgba();
			Assert.Equal(1f, rgba.Red, precision: 3);
			Assert.Equal(0f, rgba.Green, precision: 3);
			Assert.Equal(0f, rgba.Blue, precision: 3);
		}

		[Fact]
		public void SolidColorBrush_ToGdkRgba_applies_opacity()
		{
			var brush = new SolidColorBrush(Colors.White) { Opacity = 0.5 };
			var rgba = brush.ToGdkRgba();
			Assert.Equal(0.5f, rgba.Alpha, precision: 3);
		}

		[Fact]
		public void Brushes_Red_has_Red_color()
		{
			Assert.Equal(Colors.Red, Brushes.Red.Color);
		}

		[Fact]
		public void Brushes_Black_has_Black_color()
		{
			Assert.Equal(Colors.Black, Brushes.Black.Color);
		}
	}
}
