namespace System.Windows.Documents;

/// <summary>Specifies how digits are aligned: <c>pnum</c> (proportional) or <c>tnum</c> (tabular).</summary>
public enum FontNumeralAlignment
{
	/// <summary>Font default.</summary>
	Normal = 0,

	/// <summary>Proportional-width digits (<c>pnum=1</c>).</summary>
	Proportional = 1,

	/// <summary>Fixed-width digits (<c>tnum=1</c>).</summary>
	Tabular = 2,
}
