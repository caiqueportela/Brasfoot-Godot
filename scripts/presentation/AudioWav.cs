using Godot;

namespace Brasfoot;

public static class AudioWav
{
	public static AudioStreamWav Read(string caminho)
	{
		if (!File.Exists(caminho)) return null;
		var bytes = File.ReadAllBytes(caminho);
		if (bytes.Length < 44) return null;
		int canais = BitConverter.ToInt16(bytes, 22);
		int taxa = BitConverter.ToInt32(bytes, 24);
		int bits = BitConverter.ToInt16(bytes, 34);
		int dados = 44;
		for (int i = 12; i + 8 < bytes.Length; i++)
		{
			if (bytes[i] == 'd' && bytes[i + 1] == 'a' && bytes[i + 2] == 't' && bytes[i + 3] == 'a')
			{
				dados = i + 8;
				break;
			}
		}
		if (dados >= bytes.Length) return null;
		var pcm = new byte[bytes.Length - dados];
		Buffer.BlockCopy(bytes, dados, pcm, 0, pcm.Length);
		return new AudioStreamWav
		{
			Format = bits == 8 ? AudioStreamWav.FormatEnum.Format8Bits : AudioStreamWav.FormatEnum.Format16Bits,
			MixRate = taxa > 0 ? taxa : 44100,
			Stereo = canais > 1,
			Data = pcm
		};
	}
}
