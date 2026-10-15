using Godot;

namespace Brasfoot;

public static class Screenshot
{
	public static void Save(Node n, string nome)
	{
		var img = n.GetViewport().GetTexture().GetImage();
		string pasta = Path.Combine(ProjectSettings.GlobalizePath("res://"), "art");
		Directory.CreateDirectory(pasta);
		img.SavePng(Path.Combine(pasta, nome));
		GD.Print("foto " + nome);
	}
}
