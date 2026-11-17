using System;

namespace Gtk4.Forms
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class ParameterAttribute : Attribute
	{
		public ParameterAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}