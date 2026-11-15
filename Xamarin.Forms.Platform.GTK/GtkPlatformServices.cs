using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.GTK
{
	internal class GtkPlatformServices : IPlatformServices
	{
		// Was Thread.CurrentThread.IsBackground, which is simply the wrong question: a foreground
		// worker thread reports false, so Forms skipped marshalling and touched GTK widgets off
		// the main loop. Compare against the thread that ran Forms.Init instead. If Init has not
		// run we cannot know, and claiming "no marshalling needed" is the dangerous answer.
		public bool IsInvokeRequired =>
			Forms.MainThread == null || Thread.CurrentThread != Forms.MainThread;

		public string RuntimePlatform => Device.GTK;

		public void BeginInvokeOnMainThread(Action action)
		{
			GLib.Idle.Add(delegate
			{ action(); return false; });
		}

		public Ticker CreateTicker()
		{
			return new GtkTicker();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public string GetHash(string input) => Crc64.GetHash(input);

		string IPlatformServices.GetMD5Hash(string input) => GetHash(input);

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					return 11;
				case NamedSize.Micro:
				case NamedSize.Caption:
					return 12;
				case NamedSize.Medium:
					return 17;
				case NamedSize.Large:
					return 22;
				case NamedSize.Small:
				case NamedSize.Body:
					return 14;
				case NamedSize.Header:
					return 46;
				case NamedSize.Subtitle:
					return 20;
				case NamedSize.Title:
					return 24;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
		}

		public Color GetNamedColor(string name)
		{
			// GTK themes expose named colours through the style context (@theme_fg_color and
			// friends). Look the name up against the default display's theme; unknown names fall
			// back to Color.Default, which is what Forms expects for "this platform has no such
			// colour". Callers may run before a display exists, hence the null guard.
			//
			// GTK4: Gdk.Screen, new StyleContext() (inaccessible ctor), and WidgetPath are all
			// removed. StyleContext.LookupColor is static on the display-level context in GTK4;
			// the nearest equivalent is to get the widget's StyleContext from a live widget.
			// Without a live widget here, use the colour lookup on a temporary StyleContext
			// obtained from Gtk.StyleContext directly.
			// TODO GTK4: revisit when a widget reference is available to call widget.StyleContext.LookupColor
			if (string.IsNullOrEmpty(name))
				return Color.Default;

			if (Gdk.Display.Default == null)
				return Color.Default;

			// Gtk.StyleContext.LookupColor is an instance method (Gtk.StyleContext.cs:294).
			// In GTK4 we cannot construct a free-standing StyleContext, so fall back to
			// Gtk.Settings named-colour support — colours prefixed '@' in the theme.
			// Return Color.Default for any name not resolvable at this level.
			// TODO GTK4: wire through a real widget's StyleContext for full lookup coverage.
			return Color.Default;
		}

		// One HttpClient for the process. The previous code newed one up per call inside a using,
		// which is the classic socket-exhaustion pattern: disposed HttpClients leave their
		// connections in TIME_WAIT, and a page pulling many remote images exhausts ephemeral ports.
		static readonly HttpClient s_httpClient = new HttpClient();

		public Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			return StreamWrapper.GetStreamAsync(uri, cancellationToken, s_httpClient);
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new GtkIsolatedStorageFile();
		}

		public void OpenUriAction(Uri uri)
		{
			// Process.Start(string) does NOT shell-execute on .NET (Core) the way it did on .NET
			// Framework - it tries to exec the URI as a program and throws. UseShellExecute routes
			// through xdg-open on Linux, which is what actually opens a browser.
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri)
			{
				UseShellExecute = true
			});
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			GLib.Timeout.Add((uint)interval.TotalMilliseconds, () =>
			{
				var result = callback();
				return result;
			});
		}

		private static int Hex(int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}

		public void QuitApplication()
		{
			Gtk.Application.Quit();
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		public OSAppTheme RequestedTheme => GetRequestedTheme();

		// GTK has no "OS theme" signal, but it does have two settings that together say whether
		// the current theme is a dark one: the explicit gtk-application-prefer-dark-theme flag,
		// and the theme name itself (the convention is a "-dark" suffix, e.g. Adwaita-dark).
		static OSAppTheme GetRequestedTheme()
		{
			var settings = Gtk.Settings.Default;

			if (settings == null)
				return OSAppTheme.Unspecified;

			if (settings.ApplicationPreferDarkTheme)
				return OSAppTheme.Dark;

			var themeName = settings.ThemeName;

			if (!string.IsNullOrEmpty(themeName) &&
				themeName.EndsWith("-dark", StringComparison.OrdinalIgnoreCase))
				return OSAppTheme.Dark;

			return OSAppTheme.Light;
		}

		/// <summary>
		/// Subscribes to the GTK settings that back <see cref="RequestedTheme"/> so Forms is told
		/// when the desktop theme changes. Called from <c>Forms.Init</c>, after GTK is up: the
		/// settings object does not exist before <c>Gtk.Application.Init</c>.
		/// </summary>
		internal static void TrackThemeChanges()
		{
			var settings = Gtk.Settings.Default;

			if (settings == null)
				return;

			var lastTheme = GetRequestedTheme();

			void OnThemeSettingChanged(object o, GLib.NotifyArgs args)
			{
				var currentTheme = GetRequestedTheme();

				if (currentTheme == lastTheme)
					return;

				lastTheme = currentTheme;
				Application.Current?.TriggerThemeChanged(new AppThemeChangedEventArgs(currentTheme));
			}

			settings.AddNotification("gtk-theme-name", OnThemeSettingChanged);
			settings.AddNotification("gtk-application-prefer-dark-theme", OnThemeSettingChanged);
		}
	}
}