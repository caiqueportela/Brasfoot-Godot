using Godot;

namespace Brasfoot;

public static class SyncQueue
{
	public static void Enqueue(string caminho)
	{
		if (Preferences.Instance == null || !Preferences.Instance.Settings.CloudSync) return;
		string origem = Paths.AbsolutePath(caminho);
		if (!File.Exists(origem)) return;
		string pasta = Path.Combine(OS.GetUserDataDir(), "nuvem");
		Directory.CreateDirectory(pasta);
		string destino = Path.Combine(pasta, Path.GetFileName(origem));
		File.Copy(origem, destino, true);
		Preferences.Instance.Settings.PendingFile = Path.GetFileName(origem);
		Preferences.Instance.Save();
	}
}
