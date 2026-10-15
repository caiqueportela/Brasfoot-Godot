using Godot;

namespace Brasfoot;

public static class EditorIcons
{
	static readonly Dictionary<string, string> arquivos = new()
	{
		["add"] = "iconadd.png",
		["pencil"] = "iconed.png",
		["x"] = "icondel.png",
		["home"] = "home.png",
		["quit"] = "exit.png",
		["search"] = "icon_search.png",
		["people"] = "player_shirt_tot.png",
		["shirt_green"] = "player_shirt_greensmall.png",
		["shirt_gray"] = "player_shirt_graysmall.png",
		["star"] = "estrela.png",
		["star_red"] = "estrelared.png",
		["stadium"] = "estadio_icon16.png",
		["coach"] = "iconcoach16.png",
		["check"] = "greencheck.png",
		["empty"] = "camisaVazia.png",
		["up"] = "uparray.png",
		["down"] = "downarray.png",
	};

	static readonly Dictionary<string, Texture2D> cache = new();

	public static Texture2D Get(string nome)
	{
		if (cache.TryGetValue(nome, out var pronta)) return pronta;
		if (!arquivos.TryGetValue(nome, out var arquivo)) return null;
		var tex = GD.Load<Texture2D>("res://art/icons/" + arquivo);
		if (tex == null) return null;
		cache[nome] = tex;
		return tex;
	}
}
