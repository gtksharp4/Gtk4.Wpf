using System.Reflection;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ContentPositionTests
{
	[Fact]
	public void Missing_is_a_distinct_position_and_TextPointer_derives_from_ContentPosition()
	{
		Assert.NotNull(ContentPosition.Missing);
		Assert.False(ContentPosition.Missing is TextPointer);
		Assert.True(typeof(TextPointer).IsSubclassOf(typeof(ContentPosition)));
	}

	[Fact]
	public void The_constructor_is_protected()
	{
		var ctor = typeof(ContentPosition).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
			System.Type.EmptyTypes, null);
		Assert.True(ctor!.IsFamily);
	}
}
