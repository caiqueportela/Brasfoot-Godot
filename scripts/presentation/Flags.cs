using Godot;

namespace Brasfoot;

public static class Flags
{
	static readonly Dictionary<string, Texture2D> cache = new();

	public static Texture2D CountryFlag(int id)
	{
		string codigo = Catalog.Instance?.CatalogData?.CountryCode(id);
		if (string.IsNullOrWhiteSpace(codigo)) return null;
		return Load("countries/" + codigo + ".png", "p" + codigo);
	}

	public static Texture2D StateFlag(int id)
	{
		string codigo = Catalog.Instance?.CatalogData?.StateCode(id);
		if (string.IsNullOrWhiteSpace(codigo)) return null;
		return Load("states/" + codigo + ".png", "e" + codigo);
	}

	static Texture2D Load(string relativo, string chave)
	{
		if (cache.TryGetValue(chave, out var pronta)) return pronta;
		string caminho = "res://art/flags/" + relativo;
		if (!ResourceLoader.Exists(caminho)) return null;
		var tex = GD.Load<Texture2D>(caminho);
		if (tex == null) return null;
		cache[chave] = tex;
		return tex;
	}
}
