using Gtk4.Forms.Controls.Tests;
using Gtk4.Forms.Internals;
using Xunit.Sdk;

namespace Gtk4.Forms.Controls.GalleryPages.PlatformTestsGallery
{
	[Preserve(AllMembers = true)]
	public class TestNameContainsFilter : ITestCaseFilter
	{
		string _substring;

		public TestNameContainsFilter(string substring) => _substring = substring;

		public bool Match(ITestCase testCase)
		{
			return testCase.TestCaseDisplayName.Contains(_substring);
		}
	}
}
