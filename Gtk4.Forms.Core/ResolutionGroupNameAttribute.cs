using System;

namespace Gtk4.Forms
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class ResolutionGroupNameAttribute : Attribute
	{
		public ResolutionGroupNameAttribute(string name)
		{
			ShortName = name;
		}

		internal string ShortName { get; private set; }
	}
}