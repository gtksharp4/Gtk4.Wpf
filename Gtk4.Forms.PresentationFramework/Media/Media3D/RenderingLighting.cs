using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>
/// One shaded material layer of a triangle: a base colour (or textured brush) and a light multiplier.
/// </summary>
struct ShadedLayer
{
	internal Brush? TextureBrush;
	internal double R, G, B, A;
	internal double LightR, LightG, LightB;
	internal bool Additive;
}

/// <summary>Evaluates ambient, directional, point and spot lights (Blinn-Phong specular) at a surface point.</summary>
static class SceneLighting
{
	internal static void Shade(
		Material? material, Point3D p, Vector3D normal, Point3D eye, List<SceneLight> lights,
		List<ShadedLayer> output)
	{
		switch (material)
		{
			case MaterialGroup group:
				foreach (var child in Media3DProps.Peek<MaterialCollection>(group, MaterialGroup.ChildrenProperty))
					Shade(child, p, normal, eye, lights, output);
				break;
			case DiffuseMaterial d:
			{
				var layer = Base(d.Brush, d.Color);
				Ambient(lights, d.AmbientColor, ref layer);
				Diffuse(lights, p, normal, ref layer);
				output.Add(layer);
				break;
			}
			case EmissiveMaterial e:
			{
				var layer = Base(e.Brush, e.Color);
				layer.LightR = layer.LightG = layer.LightB = 1;
				layer.Additive = output.Count > 0;
				output.Add(layer);
				break;
			}
			case SpecularMaterial s:
			{
				var layer = Base(s.Brush, s.Color);
				Specular(lights, p, normal, eye, s.SpecularPower, ref layer);
				layer.Additive = true;
				output.Add(layer);
				break;
			}
		}
	}

	static ShadedLayer Base(Brush? brush, Color tint)
	{
		var layer = new ShadedLayer { R = tint.R / 255.0, G = tint.G / 255.0, B = tint.B / 255.0, A = 1 };
		if (brush == null)
		{
			layer.A = 0;
			return layer;
		}
		if (brush is SolidColorBrush solid)
		{
			var c = solid.Color;
			layer.R *= c.R / 255.0;
			layer.G *= c.G / 255.0;
			layer.B *= c.B / 255.0;
			layer.A = c.A / 255.0 * solid.Opacity;
		}
		else
		{
			layer.TextureBrush = brush;
			layer.A = brush.Opacity;
		}
		return layer;
	}

	static void Ambient(List<SceneLight> lights, Color ambient, ref ShadedLayer layer)
	{
		foreach (var l in lights)
		{
			if (!(l.Source is AmbientLight)) continue;
			var c = l.Source.Color;
			layer.LightR += c.R / 255.0 * ambient.R / 255.0;
			layer.LightG += c.G / 255.0 * ambient.G / 255.0;
			layer.LightB += c.B / 255.0 * ambient.B / 255.0;
		}
	}

	static void Diffuse(List<SceneLight> lights, Point3D p, Vector3D n, ref ShadedLayer layer)
	{
		foreach (var l in lights)
		{
			if (!Incoming(l, p, out var toLight, out var strength)) continue;
			double k = Math.Max(0, Vector3D.DotProduct(n, toLight)) * strength;
			var c = l.Source.Color;
			layer.LightR += c.R / 255.0 * k;
			layer.LightG += c.G / 255.0 * k;
			layer.LightB += c.B / 255.0 * k;
		}
	}

	static void Specular(
		List<SceneLight> lights, Point3D p, Vector3D n, Point3D eye, double power, ref ShadedLayer layer)
	{
		var toEye = eye - p;
		toEye.Normalize();
		foreach (var l in lights)
		{
			if (!Incoming(l, p, out var toLight, out var strength)) continue;
			var half = toLight + toEye;
			if (half.LengthSquared == 0) continue;
			half.Normalize();
			double k = Math.Pow(Math.Max(0, Vector3D.DotProduct(n, half)), power) * strength;
			if (Vector3D.DotProduct(n, toLight) <= 0) continue;
			var c = l.Source.Color;
			layer.LightR += c.R / 255.0 * k;
			layer.LightG += c.G / 255.0 * k;
			layer.LightB += c.B / 255.0 * k;
		}
	}

	/// <summary>
	/// Direction to a non-ambient light and its attenuation (and cone) factor at <paramref name="p"/>.
	/// </summary>
	static bool Incoming(SceneLight l, Point3D p, out Vector3D toLight, out double strength)
	{
		toLight = default;
		strength = 0;
		switch (l.Source)
		{
			case DirectionalLight _:
				toLight = -l.Direction;
				toLight.Normalize();
				strength = 1;
				return true;
			case PointLightBase point:
			{
				toLight = l.Position - p;
				double d = toLight.Length;
				if (d > point.Range) return false;
				if (d > 0) toLight /= d;
				strength = 1.0 / (point.ConstantAttenuation + point.LinearAttenuation * d
					+ point.QuadraticAttenuation * d * d);
				if (l.Source is SpotLight spot) strength *= Cone(spot, l.Direction, -toLight);
				return strength > 0;
			}
			default:
				return false;
		}
	}

	static double Cone(SpotLight spot, Vector3D axis, Vector3D fromLight)
	{
		axis.Normalize();
		double cos = Vector3D.DotProduct(axis, fromLight);
		double outer = Math.Cos(spot.OuterConeAngle * Math.PI / 360.0);
		double inner = Math.Cos(Math.Min(spot.InnerConeAngle, spot.OuterConeAngle) * Math.PI / 360.0);
		if (cos <= outer) return 0;
		if (cos >= inner) return 1;
		return (cos - outer) / (inner - outer);
	}
}
