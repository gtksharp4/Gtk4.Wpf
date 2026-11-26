namespace System.Windows.Media;

public abstract partial class Visual
{
	/// <summary>Hit-tests this visual's own content; <c>null</c> for no hit.</summary>
	protected virtual HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters) => null;

	/// <summary>Hit-tests this visual's own content against a geometry; <c>null</c> for no hit.</summary>
	protected virtual GeometryHitTestResult? HitTestCore(GeometryHitTestParameters hitTestParameters) => null;

	internal HitTestResult? InvokeHitTestCore(PointHitTestParameters p) => HitTestCore(p);

	internal GeometryHitTestResult? InvokeHitTestCore(GeometryHitTestParameters p) => HitTestCore(p);

	/// <summary>False for UIElements that are collapsed/hidden or not hit-test visible (prunes the subtree).</summary>
	internal virtual bool ParticipatesInHitTest => true;
}
