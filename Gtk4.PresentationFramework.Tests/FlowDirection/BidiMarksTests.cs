using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

public class BidiMarksTests
{
	static string Here([CallerFilePath] string path = "") => path;

	[Fact]
	public void The_constants_are_the_named_code_points()
	{
		Assert.Equal(0x2066, (int)BidiMarks.Lri);
		Assert.Equal(0x2067, (int)BidiMarks.Rli);
		Assert.Equal(0x2069, (int)BidiMarks.Pdi);
		Assert.Equal(0x200E, (int)BidiMarks.Lrm);
		Assert.Equal(0x200F, (int)BidiMarks.Rlm);
	}

	[Fact]
	public void The_source_spells_the_invisible_characters_as_escapes()
	{
		string source = Path.Combine(Path.GetDirectoryName(Here())!, "..", "..", "Gtk4.PresentationFramework",
			"Documents", "BidiMarks.cs");
		foreach (char c in File.ReadAllText(source))
			Assert.True(c < 0x80, $"BidiMarks.cs holds a raw U+{(int)c:X4}; write it as an escape");
	}
}
