using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	// Called from RegisterBuiltIns (not a field initializer) so it runs inside the static constructor.
	static void RegisterMedia3DBuiltIns()
	{
		Register<Point3D>(Point3D.Parse);
		Register<Vector3D>(Vector3D.Parse);
		Register<Size3D>(Size3D.Parse);
		Register<Point4D>(Point4D.Parse);
		Register<Quaternion>(Quaternion.Parse);
		Register<Matrix3D>(Matrix3D.Parse);
		Register<Rect3D>(Rect3D.Parse);
		Register<Point3DCollection>(Point3DCollection.Parse);
		Register<Vector3DCollection>(Vector3DCollection.Parse);
		Register<Int32Collection>(Int32Collection.Parse);
		Register<KeyTime>(raw => (KeyTime)new KeyTimeConverter().ConvertFrom(null, null, raw));
		Register<KeySpline>(raw => (KeySpline)new KeySplineConverter().ConvertFrom(null, null, raw));
	}
}
