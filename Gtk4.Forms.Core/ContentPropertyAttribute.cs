//
// ContentPropertyAttribute.cs
//
// Author:
//       Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2013 S. Delcroix
//

using System;

namespace Gtk4.Forms
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ContentPropertyAttribute : Attribute
	{
		internal static string[] ContentPropertyTypes = { "Gtk4.Forms.ContentPropertyAttribute", "System.Windows.Markup.ContentPropertyAttribute" };

		public ContentPropertyAttribute(string name) => Name = name;

		public string Name { get; }
	}
}