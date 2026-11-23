using System.Text;

namespace System.Windows.Xps.Packaging;

/// <summary>Reads the family name of a font part (an obfuscated or plain TrueType file) in an XPS package.</summary>
static class XpsFontNames
{
	/// <summary>
	/// The font family of <paramref name="fontUri"/> in <paramref name="package"/>, or null if unreadable.
	/// </summary>
	internal static string? FamilyOf(XpsPackage? package, Uri? fontUri)
	{
		if (package == null || fontUri == null)
			return null;
		try
		{
			string part = fontUri.IsAbsoluteUri ? fontUri.AbsolutePath : fontUri.OriginalString;
			part = XpsPackage.ResolveTarget("/", part);
			var data = package.ReadPart(part);
			if (part.EndsWith(".odttf", StringComparison.OrdinalIgnoreCase))
				Deobfuscate(data, part);
			return FamilyName(data);
		}
		catch (Exception ex) when (ex is System.IO.IOException || ex is FormatException
			|| ex is ArgumentException || ex is IndexOutOfRangeException)
		{
			return null;
		}
	}

	/// <summary>XORs the first 32 bytes with the GUID in the part name, as the XPS specification defines.</summary>
	internal static void Deobfuscate(byte[] data, string partName)
	{
		string name = partName.Substring(partName.LastIndexOf('/') + 1);
		string hex = name.Substring(0, name.LastIndexOf('.')).Replace("-", string.Empty);
		if (hex.Length != 32)
			throw new FormatException("The font part name is not a GUID.");
		var key = new byte[16];
		for (int i = 0; i < 16; i++)
			key[15 - i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		for (int i = 0; i < 32 && i < data.Length; i++)
			data[i] ^= key[i % 16];
	}

	internal static string? FamilyName(byte[] font)
	{
		int tables = U16(font, 4);
		for (int i = 0; i < tables; i++)
		{
			int record = 12 + i * 16;
			if (Encoding.ASCII.GetString(font, record, 4) == "name")
				return ReadName(font, (int)U32(font, record + 8));
		}
		return null;
	}

	static string? ReadName(byte[] font, int table)
	{
		int count = U16(font, table + 2), strings = table + U16(font, table + 4);
		string? family = null;
		for (int i = 0; i < count; i++)
		{
			int r = table + 6 + i * 12;
			int platform = U16(font, r), id = U16(font, r + 6), length = U16(font, r + 8), offset = U16(font, r + 10);
			if (id != 1 && id != 16)
				continue;
			string value = platform == 1
				? Encoding.ASCII.GetString(font, strings + offset, length)
				: Encoding.BigEndianUnicode.GetString(font, strings + offset, length);
			if (platform != 3 && platform != 1)
				continue;
			if (id == 16 || family == null)
				family = value;
		}
		return family;
	}

	static int U16(byte[] d, int o) => d[o] << 8 | d[o + 1];

	static uint U32(byte[] d, int o) => (uint)(d[o] << 24 | d[o + 1] << 16 | d[o + 2] << 8 | d[o + 3]);
}
