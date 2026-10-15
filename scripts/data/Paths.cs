using Godot;

namespace Brasfoot;

public static class Paths
{
	public const int FormatVersion = 2;
	public const string CatalogPath = "res://catalog/brasfoot.res";
	public const string SettingsPath = "user://settings.res";
	public const string Boot = "res://scenes/Boot.tscn";
	public const string Menu = "res://scenes/menu/MainMenu.tscn";
	public const string Settings = "res://scenes/menu/Settings.tscn";
	public const string NewGame = "res://scenes/career/NewGame.tscn";
	public const string Load = "res://scenes/career/LoadGame.tscn";
	public const string Office = "res://scenes/career/Office.tscn";
	public const string Save = "res://scenes/career/SaveCareer.tscn";
	public const string Dismissal = "res://scenes/career/Dismissal.tscn";
	public const string Match = "res://scenes/match/Match.tscn";
	public const string SeasonEnd = "res://scenes/season/SeasonEnd.tscn";
	public const string Editor = "res://scenes/editor/SquadEditor.tscn";

	public static string AbsolutePath(string caminho)
	{
		if (caminho.StartsWith("user://"))
		{
			string relativo = caminho["user://".Length..].Replace('/', Path.DirectorySeparatorChar);
			return Path.Combine(OS.GetUserDataDir(), relativo);
		}
		return ProjectSettings.GlobalizePath(caminho);
	}

	public static string SavesFolder()
	{
		string pasta = Path.Combine(OS.GetUserDataDir(), "saves");
		Directory.CreateDirectory(pasta);
		return pasta;
	}

	public static string ArtFile(string relative) => ProjectSettings.GlobalizePath("res://art/" + relative);

	public static string UserSave(string arquivo) => "user://saves/" + arquivo;
}
