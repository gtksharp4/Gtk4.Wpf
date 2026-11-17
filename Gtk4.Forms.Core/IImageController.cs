using System;

namespace Gtk4.Forms
{
	public interface IImageController : IViewController
	{
		void SetIsLoading(bool isLoading);
		bool GetLoadAsAnimation();
	}
}