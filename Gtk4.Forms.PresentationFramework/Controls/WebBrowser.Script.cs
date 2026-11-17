using System;
using System.Text.Json;

namespace System.Windows.Controls;

public partial class WebBrowser
{
	WebBrowserDom? _dom;

	/// <summary>
	/// DOM-access API: query and manipulate the current page's DOM (read/write element properties,
	/// attributes, innerHTML/innerText, form values, click, evaluate arbitrary JS). Backed by the
	/// host's script evaluator — real values through CefSharp on Windows, <c>null</c> reads through
	/// Photino (which has no synchronous JS eval).
	/// </summary>
	public WebBrowserDom Dom => _dom ??= new WebBrowserDom(js => Host.ExecuteScript(js));

	/// <summary>
	/// Evaluates <paramref name="scriptName"/> in the current page.
	/// Routes to <see cref="IWebBrowserHost.ExecuteScript"/>; through the default
	/// Photino host this returns <c>null</c> (see <see cref="IWebBrowserHost"/>).
	/// <para>
	/// Args are JSON-encoded: strings are quoted and escaped; numbers and booleans
	/// are passed as literals.  Other types fall back to their <c>ToString()</c>.
	/// </para>
	/// </summary>
	public object? InvokeScript(string scriptName, params object[] args)
	{
		string script;
		if (args.Length == 0)
		{
			script = scriptName;
		}
		else
		{
			var encodedArgs = new string[args.Length];
			for (int i = 0; i < args.Length; i++)
				encodedArgs[i] = EncodeScriptArg(args[i]);
			script = $"{scriptName}({string.Join(",", encodedArgs)})";
		}
		return Host.ExecuteScript(script);
	}

	// ── Script arg encoder (B6) ───────────────────────────────────────────────

	static string EncodeScriptArg(object? arg)
	{
		if (arg == null) return "null";
		if (arg is bool b) return b ? "true" : "false";
		if (arg is string s) return JsonSerializer.Serialize(s); // quoted + escaped
		// Numeric types: use invariant string; they don't need quoting.
		if (arg is int || arg is long || arg is double || arg is float || arg is decimal)
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", arg);
		// Fallback: ToString() unquoted — behaviour documented on InvokeScript.
		return arg.ToString() ?? "null";
	}
}