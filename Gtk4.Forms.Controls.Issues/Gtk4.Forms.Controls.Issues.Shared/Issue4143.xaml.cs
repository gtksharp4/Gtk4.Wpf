using System;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4143, "Span inaccuracies", PlatformAffected.Android)]
	public partial class Issue4143 : TestContentPage
	{
		Color[] _colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Brown, Color.Purple, Color.Orange, Color.Gray };
		Random _rand = new Random();

		protected override void Init()
		{
#if APP
			InitializeComponent();
#endif
		}

#if APP
		void OnLink1Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link1);
		}
		void OnLink2Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link2);
		}
		void OnLink3Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link3);
		}
		void OnLink4Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link4);
		}
		void OnLink5Tapped(object sender, EventArgs e)
		{
			SetRandomBackgroundColor(Link5);
		}
#endif

		void SetRandomBackgroundColor(Span span)
		{
			var oldColor = span.BackgroundColor;
			Color newColor;
			do
			{
				newColor = _colors[_rand.Next(_colors.Length)];
			} while (oldColor == newColor);

			span.BackgroundColor = newColor;
		}
	}
}
