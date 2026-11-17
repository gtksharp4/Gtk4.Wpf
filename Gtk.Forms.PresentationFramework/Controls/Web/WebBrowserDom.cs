using System;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace System.Windows.Controls;

/// <summary>
/// DOM-access API for <see cref="WebBrowser"/>. Each operation runs JavaScript in the current page
/// through the browser host's script evaluator (<see cref="IWebBrowserHost.ExecuteScript"/>).
/// </summary>
/// <remarks>
/// Results are returned as strings — the host's eval result. The CefSharp backend (Windows) returns
/// real values; the Photino backend has no synchronous JS eval and returns <c>null</c> for reads
/// (writes still run). Every id/selector/name/value argument is JSON-encoded before being embedded
/// in the script, so quotes and special characters are escaped and there is no script injection.
/// </remarks>
public sealed class WebBrowserDom
{
	readonly Func<string, string?> _evaluate;

	internal WebBrowserDom(Func<string, string?> evaluate) => _evaluate = evaluate;

	/// <summary>Evaluates an arbitrary JavaScript expression and returns its result.</summary>
	public string? Evaluate(string expression) => _evaluate(expression);

	// ── Document ────────────────────────────────────────────────────────────────

	/// <summary>The document title (<c>document.title</c>).</summary>
	public string? Title => _evaluate("document.title");

	/// <summary>The document URL (<c>document.URL</c>).</summary>
	public string? Url => _evaluate("document.URL");

	/// <summary>The document ready state (<c>document.readyState</c>).</summary>
	public string? ReadyState => _evaluate("document.readyState");

	/// <summary>The full document HTML (<c>document.documentElement.outerHTML</c>).</summary>
	public string? GetDocumentHtml() => _evaluate("document.documentElement.outerHTML");

	// ── Element by id ─────────────────────────────────────────────────────────────

	/// <summary>Reads a property of the element with id <paramref name="id"/>.</summary>
	public string? GetElementProperty(string id, string property) =>
		_evaluate($"document.getElementById({J(id)})?.[{J(property)}]");

	/// <summary>Sets a property of the element with id <paramref name="id"/>.</summary>
	public void SetElementProperty(string id, string property, object? value) =>
		_evaluate($"(()=>{{const e=document.getElementById({J(id)});if(e)e[{J(property)}]={J(value)};}})()");

	// ── Element by CSS selector ───────────────────────────────────────────────────

	/// <summary>Reads <c>innerHTML</c> of the first element matching <paramref name="selector"/>.</summary>
	public string? GetInnerHtml(string selector) =>
		_evaluate($"document.querySelector({J(selector)})?.innerHTML");

	/// <summary>Sets <c>innerHTML</c> of the first element matching <paramref name="selector"/>.</summary>
	public void SetInnerHtml(string selector, string html) =>
		_evaluate($"(()=>{{const e=document.querySelector({J(selector)});if(e)e.innerHTML={J(html)};}})()");

	/// <summary>Reads <c>innerText</c> of the first element matching <paramref name="selector"/>.</summary>
	public string? GetInnerText(string selector) =>
		_evaluate($"document.querySelector({J(selector)})?.innerText");

	/// <summary>Sets <c>innerText</c> of the first element matching <paramref name="selector"/>.</summary>
	public void SetInnerText(string selector, string text) =>
		_evaluate($"(()=>{{const e=document.querySelector({J(selector)});if(e)e.innerText={J(text)};}})()");

	/// <summary>Reads an attribute of the first element matching <paramref name="selector"/>.</summary>
	public string? GetAttribute(string selector, string name) =>
		_evaluate($"document.querySelector({J(selector)})?.getAttribute({J(name)})");

	/// <summary>Sets an attribute of the first element matching <paramref name="selector"/>.</summary>
	public void SetAttribute(string selector, string name, string value) =>
		_evaluate($"(()=>{{const e=document.querySelector({J(selector)});if(e)e.setAttribute({J(name)},{J(value)});}})()");

	/// <summary>Reads the <c>value</c> of the first form element matching <paramref name="selector"/>.</summary>
	public string? GetValue(string selector) =>
		_evaluate($"document.querySelector({J(selector)})?.value");

	/// <summary>Sets the <c>value</c> of the first form element matching <paramref name="selector"/>.</summary>
	public void SetValue(string selector, string value) =>
		_evaluate($"(()=>{{const e=document.querySelector({J(selector)});if(e)e.value={J(value)};}})()");

	/// <summary>Clicks the first element matching <paramref name="selector"/>.</summary>
	public void Click(string selector) =>
		_evaluate($"document.querySelector({J(selector)})?.click()");

	/// <summary>True when at least one element matches <paramref name="selector"/>.</summary>
	public bool Exists(string selector) =>
		string.Equals(
			_evaluate($"String(document.querySelector({J(selector)})!==null)"),
			"true", StringComparison.OrdinalIgnoreCase);

	// JSON-encode so the value becomes a single quoted literal with quotes, backslashes and control
	// characters escaped — injection-safe inside a double-quoted JS string. UnsafeRelaxedJsonEscaping
	// leaves <, >, & literal (we are building JS, not HTML), keeping the generated script readable.
	static readonly JsonSerializerOptions JsonOptions =
		new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	static string J(object? value) => JsonSerializer.Serialize(value, JsonOptions);
}
