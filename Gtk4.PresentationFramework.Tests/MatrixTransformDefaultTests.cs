using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class MatrixTransformDefaultTests
{
	[Fact]
	public void A_default_MatrixTransform_is_the_identity()
	{
		var transform = new MatrixTransform();

		Assert.True(transform.Value.IsIdentity);
		Assert.True(transform.Matrix.IsIdentity);
	}
}
