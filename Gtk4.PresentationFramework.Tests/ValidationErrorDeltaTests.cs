using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>SetErrorsFor raises events only for errors that changed, adding new ones before removing stale ones.</summary>
public class ValidationErrorDeltaTests
{
	sealed class Target : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	[Fact]
	public void Changing_one_of_several_errors_notifies_only_that_pair_added_first()
	{
		var el = new Target();
		var owner = new object();
		var a = new ValidationError(null, "a");
		var b = new ValidationError(null, "b");
		var log = new List<string>();
		Validation.AddErrorHandler(el, (_, e) => log.Add(e.Action + ":" + e.Error.ErrorContent));

		Validation.SetErrorsFor(el, owner, new[] { a, b }, true);
		log.Clear();
		var collection = Validation.GetErrors(el);
		Validation.SetErrorsFor(el, owner, new[] { a, new ValidationError(null, "c") }, true);

		Assert.Equal(new[] { "Added:c", "Removed:b" }, log);
		Assert.Same(collection, Validation.GetErrors(el));
		Assert.Same(a, collection[0]);
		Assert.Equal(new object[] { "a", "c" }, collection.Select(x => x.ErrorContent).ToArray());
	}
}
