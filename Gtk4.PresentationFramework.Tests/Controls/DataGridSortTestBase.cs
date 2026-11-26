namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>Shared POCO fixture for the DataGrid column-sort tests.</summary>
public abstract class DataGridSortTestBase
{
	protected sealed class Person
	{
		public string Name { get; }
		public int Age { get; }
		public Person(string name, int age) { Name = name; Age = age; }
	}
}
