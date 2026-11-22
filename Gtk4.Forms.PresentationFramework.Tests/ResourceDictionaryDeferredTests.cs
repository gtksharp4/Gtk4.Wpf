using System;
using System.Linq;
using System.Windows;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Dictionary values are built on first lookup, as WPF defers them (ExpenseItIntro).</summary>
public class ResourceDictionaryDeferredTests
{
	[Fact]
	public void An_unread_deferred_value_never_runs_its_factory()
	{
		int runs = 0;
		var d = new ResourceDictionary();
		d.SetDeferred("K", () => { runs++; throw new InvalidOperationException("missing"); });

		Assert.True(d.ContainsKey("K"));
		Assert.Equal(1, d.Count);
		Assert.Equal(0, runs);
	}

	[Fact]
	public void Reading_a_deferred_value_with_a_missing_key_throws_and_retries()
	{
		int runs = 0;
		var d = new ResourceDictionary();
		d.SetDeferred("K", () => { runs++; throw new InvalidOperationException("Resource with key 'X' not found."); });

		Assert.Throws<InvalidOperationException>(() => d["K"]);
		Assert.Throws<InvalidOperationException>(() => d.TryGetValue("K", out _));
		Assert.Equal(2, runs);
	}

	[Fact]
	public void A_deferred_value_is_built_once_and_forward_references_resolve()
	{
		int runs = 0;
		var d = new ResourceDictionary();
		d.SetDeferred("A", () => { runs++; return new object[] { d["B"]! }; });
		d["B"] = "b";

		var first = d["A"];
		Assert.Same(first, d["A"]);
		Assert.Equal("b", ((object[])first!)[0]);
		Assert.Equal(1, runs);
	}

	[Fact]
	public void A_deferred_value_in_a_merged_dictionary_is_found_through_the_parent()
	{
		var merged = new ResourceDictionary();
		merged.SetDeferred("M", () => "m");
		var parent = new ResourceDictionary();
		parent.MergedDictionaries.Add(merged);

		Assert.Equal("m", parent["M"]);
	}

	[Fact]
	public void CopyEntriesFrom_keeps_values_deferred_and_shares_the_instance()
	{
		int runs = 0;
		var source = new ResourceDictionary();
		source.SetDeferred("K", () => { runs++; return new object(); });
		var target = new ResourceDictionary();
		target.CopyEntriesFrom(source);

		Assert.Equal(0, runs);
		Assert.Same(source["K"], target["K"]);
		Assert.Equal(1, runs);
	}

	[Fact]
	public void Enumerating_realises_the_deferred_values()
	{
		var d = new ResourceDictionary();
		d.SetDeferred("K", () => 5);

		Assert.Equal(5, d.Values.Single());
		Assert.Equal(5, d.Single().Value);
	}

	[Fact]
	public void A_value_that_reads_itself_throws_instead_of_recursing()
	{
		var d = new ResourceDictionary();
		d.SetDeferred("K", () => d["K"]);

		Assert.Throws<InvalidOperationException>(() => d["K"]);
	}

	[Fact]
	public void An_unshared_deferred_value_is_built_on_every_lookup()
	{
		int runs = 0;
		var d = new ResourceDictionary();
		d.SetDeferred("K", () => { runs++; return new object(); }, shared: false);

		Assert.NotSame(d["K"], d["K"]);
		Assert.Equal(2, runs);
		Assert.Single(d.Values);
		Assert.Equal(3, runs);
	}

	[Fact]
	public void Removing_the_realised_pair_of_an_unrealised_entry_succeeds()
	{
		var obj = new object();
		var d = new ResourceDictionary();
		d.SetDeferred("k", () => obj);

		Assert.False(d.Remove(new System.Collections.Generic.KeyValuePair<object, object?>("k", new object())));
		Assert.True(d.Remove(new System.Collections.Generic.KeyValuePair<object, object?>("k", obj)));
		Assert.False(d.ContainsKey("k"));
	}
}
