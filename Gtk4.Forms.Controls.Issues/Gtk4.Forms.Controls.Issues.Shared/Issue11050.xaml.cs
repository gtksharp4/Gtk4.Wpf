using System;
using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Shape)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11050, "[Bug][iOS][Android] Shapes: clock drawing erro", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue11050 : ContentPage
	{
		public Issue11050()
		{
#if APP

			InitializeComponent();

			Device.StartTimer(TimeSpan.FromMilliseconds(15), () =>
			{
				DateTime dateTime = DateTime.Now;
				secondHand.Angle = 6 * (dateTime.Second + dateTime.Millisecond / 1000.0);
				minuteHand.Angle = 6 * dateTime.Minute + secondHand.Angle / 60;
				hourHand.Angle = 30 * (dateTime.Hour % 12) + minuteHand.Angle / 12;

				return true;
			});

			SizeChanged += (sender, args) =>
			{
				grid.AnchorX = 0;
				grid.AnchorY = 0;
				grid.Scale = Math.Min(Width, Height) / 200;
			};
#endif
		}
	}
}
