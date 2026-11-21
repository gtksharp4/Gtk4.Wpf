using System.Windows.Documents;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.FlowDirectionTests;

public class BidiMarksTests
{
	[Fact]
	public void The_constants_are_the_named_code_points()
	{
		Assert.Equal(0x2066, (int)BidiMarks.Lri);
		Assert.Equal(0x2067, (int)BidiMarks.Rli);
		Assert.Equal(0x2069, (int)BidiMarks.Pdi);
		Assert.Equal(0x200E, (int)BidiMarks.Lrm);
		Assert.Equal(0x200F, (int)BidiMarks.Rlm);
	}
}
