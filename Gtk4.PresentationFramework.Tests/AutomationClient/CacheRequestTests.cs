using System;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class CacheRequestTests : UiaTestBase
{
	[Fact]
	public void Elements_found_under_an_active_request_carry_the_requested_properties() => Run(() =>
	{
		var f = OpenForm();
		var request = new CacheRequest();
		request.Add(AutomationElement.NameProperty);
		request.Add(AutomationElement.IsEnabledProperty);
		AutomationElement ok;
		using (request.Activate())
			ok = Find("Ok");
		f.Ok.Content = "Changed";
		f.Ok.IsEnabled = false;
		Assert.Equal("OK", ok.Cached.Name);
		Assert.True(ok.Cached.IsEnabled);
		Assert.Equal("Changed", ok.Current.Name);
		Assert.Throws<InvalidOperationException>(() => ok.Cached.ClassName);
	});

	[Fact]
	public void GetUpdatedCache_snapshots_patterns() => Run(() =>
	{
		var f = OpenForm();
		var request = new CacheRequest();
		request.Add(TogglePattern.Pattern);
		request.Add(TogglePattern.ToggleStateProperty);
		var agree = Find("Agree").GetUpdatedCache(request);
		f.Agree.IsChecked = true;
		var toggle = (TogglePattern)agree.GetCachedPattern(TogglePattern.Pattern);
		Assert.Equal(ToggleState.Off, toggle.Cached.ToggleState);
		Assert.Equal(ToggleState.On, toggle.Current.ToggleState);
		Assert.Equal(ToggleState.Off, agree.GetCachedPropertyValue(TogglePattern.ToggleStateProperty));
		Assert.False(agree.TryGetCachedPattern(InvokePattern.Pattern, out _));
		Assert.Throws<InvalidOperationException>(() => agree.GetCachedPattern(InvokePattern.Pattern));
	});

	[Fact]
	public void Cache_covers_the_children_and_gives_them_the_element_as_parent() => Run(() =>
	{
		OpenForm();
		var request = new CacheRequest { TreeScope = TreeScope.Element | TreeScope.Children };
		request.Add(AutomationElement.AutomationIdProperty);
		var panel = Find("Panel").GetUpdatedCache(request);
		Assert.Equal(7, panel.CachedChildren.Count);
		Assert.Equal("Name", panel.CachedChildren[0].Cached.AutomationId);
		Assert.Equal("Panel", panel.CachedChildren[0].CachedParent.Cached.AutomationId);
		Assert.Throws<InvalidOperationException>(() => panel.CachedParent);
	});

	[Theory]
	[InlineData(TreeScope.Parent)]
	[InlineData(TreeScope.Ancestors)]
	[InlineData(TreeScope.Element | TreeScope.Parent)]
	[InlineData((TreeScope)0)]
	public void TreeScope_rejects_scopes_WPF_rejects(TreeScope scope) =>
		Assert.Throws<ArgumentException>(() => new CacheRequest().TreeScope = scope);

	[Theory]
	[InlineData(TreeScope.Children)]
	[InlineData(TreeScope.Element | TreeScope.Descendants)]
	public void TreeScope_accepts_any_Element_Children_Descendants_combination(TreeScope scope)
	{
		var request = new CacheRequest { TreeScope = scope };
		Assert.Equal(scope, request.TreeScope);
	}

	[Fact]
	public void Request_stack_Push_Pop_and_Current() => Run(() =>
	{
		var outer = new CacheRequest();
		Assert.False(ReferenceEquals(outer, CacheRequest.Current));
		outer.Push();
		Assert.Same(outer, CacheRequest.Current);
		var inner = outer.Clone();
		inner.Push();
		Assert.Same(inner, CacheRequest.Current);
		Assert.Throws<InvalidOperationException>(() => outer.Pop());
		inner.Pop();
		outer.Pop();
		Assert.NotSame(outer, CacheRequest.Current);
	});

	[Fact]
	public void An_active_request_is_read_only() => Run(() =>
	{
		var request = new CacheRequest();
		using (request.Activate())
		{
			Assert.Throws<InvalidOperationException>(() => request.Add(AutomationElement.NameProperty));
			Assert.Throws<InvalidOperationException>(() => request.TreeScope = TreeScope.Children);
		}
		request.Add(AutomationElement.NameProperty);
		Assert.Equal(TreeScope.Element, request.TreeScope);
		Assert.Equal(AutomationElementMode.Full, request.AutomationElementMode);
	});
}
