using System;

namespace System.Windows.Navigation
{
	/// <summary>
	/// Arguments for the <see cref="NavigationService.Navigating"/> event.
	/// Set <see cref="Cancel"/> to <c>true</c> to abort the pending navigation.
	/// </summary>
	public class NavigatingCancelEventArgs : EventArgs
	{
		/// <summary>The content (or Uri) that is about to be navigated to.</summary>
		public object? Content { get; }

		/// <summary>The URI being navigated to, or <c>null</c> for object-content navigations.</summary>
		public Uri? Uri { get; }

		/// <summary>Set to <c>true</c> to cancel the navigation.</summary>
		public bool Cancel { get; set; }

		internal NavigatingCancelEventArgs(object? content, Uri? uri)
		{
			Content = content;
			Uri = uri;
		}
	}
}
