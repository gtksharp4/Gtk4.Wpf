using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class SortDescriptionTests
{
	[Fact]
	public void Object_initializer_sets_both_members()
	{
		var sd = new SortDescription { PropertyName = "Age", Direction = ListSortDirection.Descending };

		Assert.Equal("Age", sd.PropertyName);
		Assert.Equal(ListSortDirection.Descending, sd.Direction);
		Assert.False(sd.IsSealed);
	}

	[Fact]
	public void Direction_outside_the_enum_throws()
	{
		var sd = new SortDescription();

		Assert.Throws<InvalidEnumArgumentException>(() => sd.Direction = (ListSortDirection)5);
		Assert.Throws<InvalidEnumArgumentException>(() => new SortDescription("A", (ListSortDirection)(-1)));

		var viaSetter = Assert.Throws<InvalidEnumArgumentException>(() => sd.Direction = (ListSortDirection)5);
		Assert.Equal("value", viaSetter.ParamName);
		var viaCtor = Assert.Throws<InvalidEnumArgumentException>(() => new SortDescription("A", (ListSortDirection)5));
		Assert.Equal("direction", viaCtor.ParamName);
	}

	[Fact]
	public void A_description_in_a_collection_is_sealed_and_rejects_changes()
	{
		var original = new SortDescription("A", ListSortDirection.Ascending);
		var sorts = new SortDescriptionCollection { original };

		Assert.False(original.IsSealed);
		Assert.True(sorts[0].IsSealed);
		var copy = sorts[0];
		Assert.Throws<InvalidOperationException>(() => copy.PropertyName = "B");
		Assert.Throws<InvalidOperationException>(() => copy.Direction = ListSortDirection.Descending);
		original.PropertyName = "C";
		Assert.Equal("A", sorts[0].PropertyName);
	}

	[Fact]
	public void SetItem_seals_the_replacement()
	{
		var sorts = new SortDescriptionCollection { new SortDescription("A", ListSortDirection.Ascending) };
		sorts[0] = new SortDescription("B", ListSortDirection.Descending);

		Assert.True(sorts[0].IsSealed);
		Assert.Equal("B", sorts[0].PropertyName);
	}

	[Fact]
	public void Equality_compares_name_and_direction_not_sealing()
	{
		var a = new SortDescription("A", ListSortDirection.Ascending);
		var sorts = new SortDescriptionCollection { a };

		Assert.True(a == sorts[0]);
		Assert.True(a.Equals(sorts[0]));
		Assert.Equal(a.GetHashCode(), sorts[0].GetHashCode());
		Assert.True(a != new SortDescription("A", ListSortDirection.Descending));
		Assert.True(a != new SortDescription("B", ListSortDirection.Ascending));
		Assert.False(a.Equals("A"));
	}

	class Person
	{
		public string Name { get; set; } = "";
	}

	[Fact]
	public void Default_view_still_sorts_by_an_initializer_description()
	{
		var people = new List<Person> { new Person { Name = "b" }, new Person { Name = "c" }, new Person { Name = "a" } };
		var view = CollectionViewSource.GetDefaultView(people)!;

		view.SortDescriptions.Add(new SortDescription { PropertyName = "Name", Direction = ListSortDirection.Descending });

		Assert.Equal(new[] { "c", "b", "a" }, view.Cast<Person>().Select(p => p.Name).ToArray());
	}
}
