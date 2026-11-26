using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class ConverterSealingTests
{
	[Fact]
	public void Point_vector_and_matrix_converters_are_sealed()
	{
		Assert.True(typeof(PointConverter).IsSealed);
		Assert.True(typeof(VectorConverter).IsSealed);
		Assert.True(typeof(MatrixConverter).IsSealed);
	}
}
