using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;

namespace System.Windows.Data;

/// <summary>
/// Groups a set of <see cref="BindingExpression"/>s for transactional validation and
/// commit.  Call <see cref="BeginEdit"/> before changing values, then
/// <see cref="CommitEdit"/> (runs <see cref="ValidationRules"/> and writes sources)
/// or <see cref="CancelEdit"/> to discard staged values.
/// </summary>
public partial class BindingGroup
{
	// ── Configuration ────────────────────────────────────────────────────────

	/// <summary>Name used to reference this group from XAML (e.g. <c>x:Key</c>).</summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>All <see cref="BindingExpression"/>s registered with this group.</summary>
	public IList<BindingExpression> BindingExpressions { get; } =
		new ObservableCollection<BindingExpression>();

	/// <summary>Validation rules applied by <see cref="ValidateWithoutUpdate"/>.</summary>
	public IList<ValidationRule> ValidationRules { get; } =
		new ObservableCollection<ValidationRule>();

	/// <summary>
	/// Source objects that participate in this group.
	/// Each distinct <see cref="BindingExpression.ResolvedSource"/> appears once.
	/// </summary>
	public IList<object> Items
	{
		get
		{
			// Use object-identity to deduplicate sources.
			var seen = new HashSet<object>();
			var result = new List<object>();
			foreach (var expr in BindingExpressions)
			{
				var src = expr.ResolvedSource;
				if (src != null && seen.Add(src)) result.Add(src);
			}
			return result;
		}
	}

	// ── Transactional API ────────────────────────────────────────────────────

	/// <summary>
	/// Begins an edit session.  Call before changing target values.
	/// </summary>
	public void BeginEdit()
	{
		// No snapshot required: target DPs hold the staged values directly.
		// BeginEdit is a signal to callers — CommitEdit / CancelEdit complete it.
	}

	/// <summary>
	/// Runs <see cref="ValidationRules"/> and, if all pass, writes staged values
	/// to sources via each expression's <see cref="BindingExpression.UpdateSource"/>.
	/// Returns <c>true</c> on success.
	/// <para>
	/// Exception note: the write loop is not transactional — an exception thrown by a
	/// mid-loop source write is not caught and will leave subsequent expressions unwritten.
	/// Callers that require all-or-nothing semantics must wrap this call themselves.
	/// </para>
	/// </summary>
	public bool CommitEdit()
	{
		if (!ValidateWithoutUpdate()) return false;

		foreach (var expr in BindingExpressions)
			expr.UpdateSource();

		return true;
	}

	/// <summary>
	/// Discards staged values and refreshes each binding's target from its source.
	/// </summary>
	public void CancelEdit()
	{
		foreach (var expr in BindingExpressions)
			expr.UpdateTarget();
	}

	/// <summary>
	/// Calls <see cref="BindingExpression.UpdateSource"/> on each registered expression,
	/// without running validation rules first.
	/// </summary>
	public void UpdateSources()
	{
		foreach (var expr in BindingExpressions)
			expr.UpdateSource();
	}

	/// <summary>
	/// Runs all <see cref="ValidationRules"/> with the group as the value
	/// (rules read proposed values via GetValue/TryGetValue).  Returns <c>true</c> when all pass.
	/// </summary>
	public bool ValidateWithoutUpdate()
	{
		// WPF hands a group rule the group itself; the rule reads proposed values with GetValue/TryGetValue.
		foreach (var rule in ValidationRules)
		{
			ValidationResult result;
			try
			{
				result = rule.Validate(this, BindingEngine.ResolveCulture(null), this);
			}
			catch
			{
				return false;
			}
			if (!result.IsValid) return false;
		}
		return true;
	}
}
