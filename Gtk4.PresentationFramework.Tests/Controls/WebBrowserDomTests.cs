using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Tests for <see cref="WebBrowserDom"/>: the generated JavaScript (JSON-escaped, injection-safe)
/// and the pass-through of the host's eval result. No browser required — the evaluator is a
/// capturing delegate — plus one integration check through <see cref="WebBrowser.Dom"/>.
/// </summary>
public class WebBrowserDomTests
{
	string? _js;
	WebBrowserDom Dom(string? result = null) => new(js => { _js = js; return result; });

	// ── Evaluate / document ─────────────────────────────────────────────────────

	[Fact]
	public void Evaluate_passes_expression_and_returns_result()
	{
		var dom = Dom("42");
		Assert.Equal("42", dom.Evaluate("1+1"));
		Assert.Equal("1+1", _js);
	}

	[Fact]
	public void Document_accessors_emit_expected_js()
	{
		var dom = Dom();
		_ = dom.Title; Assert.Equal("document.title", _js);
		_ = dom.Url; Assert.Equal("document.URL", _js);
		_ = dom.ReadyState; Assert.Equal("document.readyState", _js);
		_ = dom.GetDocumentHtml(); Assert.Equal("document.documentElement.outerHTML", _js);
	}

	// ── Selector reads ──────────────────────────────────────────────────────────

	[Fact]
	public void GetInnerHtml_emits_querySelector_innerHTML_and_returns_result()
	{
		var dom = Dom("<b>hi</b>");
		var result = dom.GetInnerHtml("#x");
		Assert.Equal("document.querySelector(\"#x\")?.innerHTML", _js);
		Assert.Equal("<b>hi</b>", result);
	}

	[Fact]
	public void GetInnerText_GetValue_GetAttribute_emit_expected_js()
	{
		var dom = Dom();
		_ = dom.GetInnerText(".c"); Assert.Equal("document.querySelector(\".c\")?.innerText", _js);
		_ = dom.GetValue("#i"); Assert.Equal("document.querySelector(\"#i\")?.value", _js);
		_ = dom.GetAttribute("a", "href"); Assert.Equal("document.querySelector(\"a\")?.getAttribute(\"href\")", _js);
	}

	[Fact]
	public void GetElementProperty_uses_getElementById_computed_member()
	{
		var dom = Dom();
		_ = dom.GetElementProperty("myid", "value");
		Assert.Equal("document.getElementById(\"myid\")?.[\"value\"]", _js);
	}

	// ── Writes ──────────────────────────────────────────────────────────────────

	[Fact]
	public void SetInnerHtml_emits_guarded_assignment()
	{
		var dom = Dom();
		dom.SetInnerHtml("#x", "<b>hi</b>");
		Assert.Equal("(()=>{const e=document.querySelector(\"#x\");if(e)e.innerHTML=\"<b>hi</b>\";})()", _js);
	}

	[Fact]
	public void SetAttribute_and_SetValue_emit_guarded_assignment()
	{
		var dom = Dom();
		dom.SetAttribute("#x", "data-k", "v");
		Assert.Equal("(()=>{const e=document.querySelector(\"#x\");if(e)e.setAttribute(\"data-k\",\"v\");})()", _js);
		dom.SetValue("#i", "typed");
		Assert.Equal("(()=>{const e=document.querySelector(\"#i\");if(e)e.value=\"typed\";})()", _js);
	}

	[Fact]
	public void SetElementProperty_uses_bracket_member()
	{
		var dom = Dom();
		dom.SetElementProperty("myid", "checked", true);
		Assert.Equal("(()=>{const e=document.getElementById(\"myid\");if(e)e[\"checked\"]=true;})()", _js);
	}

	[Fact]
	public void Click_emits_optional_click()
	{
		Dom().Click("#btn");
		Assert.Equal("document.querySelector(\"#btn\")?.click()", _js);
	}

	// ── Exists / injection safety ───────────────────────────────────────────────

	[Theory]
	[InlineData("true", true)]
	[InlineData("false", false)]
	[InlineData(null, false)]
	public void Exists_parses_eval_result(string? evalResult, bool expected)
	{
		Assert.Equal(expected, Dom(evalResult).Exists("#x"));
	}

	[Fact]
	public void Arguments_are_json_escaped_against_injection()
	{
		Dom().GetInnerHtml("a\"]);alert(1);//");
		// The quote is escaped and the whole selector stays a single JSON string literal.
		Assert.Equal("document.querySelector(\"a\\\"]);alert(1);//\")?.innerHTML", _js);
	}

	// ── Integration: WebBrowser.Dom routes through the host ─────────────────────

	[Fact]
	public void WebBrowser_Dom_routes_through_the_host()
	{
		var original = WebBrowser.HostFactory;
		var fake = new FakeWebBrowserHost { ExecuteScriptResult = "page title" };
		WebBrowser.HostFactory = () => fake;
		try
		{
			using var wb = new WebBrowser();
			var title = wb.Dom.Title;

			Assert.Equal("document.title", fake.LastExecutedScript);
			Assert.Equal("page title", title);
		}
		finally
		{
			WebBrowser.HostFactory = original;
		}
	}
}
