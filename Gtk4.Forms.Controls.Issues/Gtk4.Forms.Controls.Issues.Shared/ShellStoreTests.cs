using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellStoreTests
	{
#if UITEST

		IApp RunningApp;
		[SetUp]
		public void ShellStoreSetup()
		{
			RunningApp = AppSetup.Setup();
			if (RunningApp.Query("SwapRoot - Store Shell").Length > 0)
				RunningApp.Tap("SwapRoot - Store Shell");
			else
				RunningApp.NavigateTo("SwapRoot - Store Shell");

			RunningApp.WaitForElement("Welcome to the HomePage");
		}

		[Test]
		public void LoadsWithoutCrashing()
		{
			RunningApp.WaitForElement("Welcome to the HomePage");
		}
#endif
	}
}
