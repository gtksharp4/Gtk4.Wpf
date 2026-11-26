using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>Direct tests for the pure <c>SizeChangeMath</c> helper.</summary>
public class SizeChangeMathTests
{
	[Fact]
	public void Width_only_change()
	{
		bool any = SizeChangeMath.Compute(new Size(10, 20), new Size(11, 20), out bool w, out bool h);
		Assert.True(any);
		Assert.True(w);
		Assert.False(h);
	}

	[Fact]
	public void Height_only_change()
	{
		bool any = SizeChangeMath.Compute(new Size(10, 20), new Size(10, 25), out bool w, out bool h);
		Assert.True(any);
		Assert.False(w);
		Assert.True(h);
	}

	[Fact]
	public void Both_axes_change()
	{
		bool any = SizeChangeMath.Compute(new Size(0, 0), new Size(100, 50), out bool w, out bool h);
		Assert.True(any);
		Assert.True(w);
		Assert.True(h);
	}

	[Fact]
	public void No_change()
	{
		bool any = SizeChangeMath.Compute(new Size(10, 20), new Size(10, 20), out bool w, out bool h);
		Assert.False(any);
		Assert.False(w);
		Assert.False(h);
	}

	[Fact]
	public void Rounding_noise_below_DBL_EPSILON_scale_is_not_a_change()
	{
		// DoubleUtil.AreClose semantics: 0.1 + 0.2 vs 0.3 differ only in the last ulp.
		bool any = SizeChangeMath.Compute(new Size(0.1 + 0.2, 5), new Size(0.3, 5), out bool w, out _);
		Assert.False(any);
		Assert.False(w);
		Assert.True(SizeChangeMath.AreClose(1e6, 1e6 + 1e-12));
		Assert.False(SizeChangeMath.AreClose(1, 1.0001));
	}
}
