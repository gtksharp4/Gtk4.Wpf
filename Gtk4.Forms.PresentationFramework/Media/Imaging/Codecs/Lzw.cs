using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>GIF variable-width LZW.</summary>
static class Lzw
{
	internal static byte[] Decode(byte[] data, int minCode, int pixelCount)
	{
		var output = new byte[pixelCount];
		int clear = 1 << minCode, end = clear + 1, size = minCode + 1, next = end + 1;
		var prefix = new int[4096];
		var suffix = new byte[4096];
		var stack = new byte[4097];
		int bits = 0, acc = 0, pos = 0, written = 0, prev = -1;
		byte first = 0;
		for (int i = 0; i < clear; i++)
			suffix[i] = (byte)i;
		while (written < pixelCount)
		{
			while (bits < size && pos < data.Length)
			{
				acc |= data[pos++] << bits;
				bits += 8;
			}
			if (bits < size)
				break;
			int code = acc & (1 << size) - 1;
			acc >>= size;
			bits -= size;
			if (code == end)
				break;
			if (code == clear)
			{
				size = minCode + 1;
				next = end + 1;
				prev = -1;
				continue;
			}
			int cur = code, sp = 0;
			if (prev == -1)
			{
				output[written++] = suffix[code];
				prev = code;
				first = suffix[code];
				continue;
			}
			if (code >= next)
			{
				stack[sp++] = first;
				cur = prev;
			}
			while (cur >= clear)
			{
				stack[sp++] = suffix[cur];
				cur = prefix[cur];
			}
			first = suffix[cur];
			stack[sp++] = first;
			if (next < 4096)
			{
				prefix[next] = prev;
				suffix[next++] = first;
				if (next == 1 << size && size < 12)
					size++;
			}
			prev = code;
			while (sp > 0 && written < pixelCount)
				output[written++] = stack[--sp];
		}
		return output;
	}

	internal static byte[] Encode(byte[] indices, int minCode)
	{
		var output = new List<byte>();
		int clear = 1 << minCode, end = clear + 1, size = minCode + 1, next = end + 1;
		int acc = 0, bits = 0;
		var dict = new Dictionary<int, int>();

		void Emit(int code)
		{
			acc |= code << bits;
			bits += size;
			while (bits >= 8)
			{
				output.Add((byte)acc);
				acc >>= 8;
				bits -= 8;
			}
		}

		Emit(clear);
		int cur = indices[0];
		for (int i = 1; i < indices.Length; i++)
		{
			int key = cur << 8 | indices[i];
			if (dict.TryGetValue(key, out int found))
			{
				cur = found;
				continue;
			}
			Emit(cur);
			if (next < 4096)
			{
				dict[key] = next++;
				if (next > 1 << size && size < 12)
					size++;
			}
			else
			{
				Emit(clear);
				dict.Clear();
				size = minCode + 1;
				next = end + 1;
			}
			cur = indices[i];
		}
		Emit(cur);
		Emit(end);
		if (bits > 0)
			output.Add((byte)acc);
		return output.ToArray();
	}
}
