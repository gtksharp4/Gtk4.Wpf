using System;
using System.Collections.Generic;
using System.Text;
using Gtk4.Forms.PlatformConfiguration;

namespace Gtk4.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Gtk4.Forms.SearchBar;

	public static class SearchBar
	{
		public static readonly BindableProperty SearchBarStyleProperty = BindableProperty.Create("SearchBarStyle", typeof(UISearchBarStyle), typeof(SearchBar), UISearchBarStyle.Default);

		public static UISearchBarStyle GetSearchBarStyle(BindableObject element)
		{
			return (UISearchBarStyle)element.GetValue(SearchBarStyleProperty);
		}

		public static void SetSearchBarStyle(BindableObject element, UISearchBarStyle style)
		{
			element.SetValue(SearchBarStyleProperty, style);
		}

		public static UISearchBarStyle GetSearchBarStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSearchBarStyle(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetSearchBarStyle(
			this IPlatformElementConfiguration<iOS, FormsElement> config, UISearchBarStyle style)
		{
			SetSearchBarStyle(config.Element, style);
			return config;
		}
	}
}
